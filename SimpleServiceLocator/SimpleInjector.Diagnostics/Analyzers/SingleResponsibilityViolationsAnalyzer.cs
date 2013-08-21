﻿#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal sealed class SingleResponsibilityViolationsAnalyzer : IContainerAnalyzer
    {
        private const int MaximumValidNumberOfDependencies = 6;

        public DiagnosticType DiagnosticType
        {
            get { return DiagnosticType.SingleResponsibilityViolation; }
        }

        public string Name
        {
            get { return "Potential Single Responsibility Violations"; }
        }

        public string GetRootDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();

            return count + " possible single responsibility " + ViolationPlural(count) + ".";
        }

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();

            return count + " possible " + ViolationPlural(count) + ".";
        }

        DiagnosticResult[] IContainerAnalyzer.Analyze(Container container)
        {
            return this.Analyze(container);
        }

        public SingleResponsibilityViolationDiagnosticResult[] Analyze(Container container)
        {
            return (
                from registration in container.GetCurrentRegistrations()
                where IsAnalyzableRegistration(registration)
                from relationship in registration.GetRelationships()
                group relationship by new { relationship.ImplementationType, registration } into g
                where g.Count() > MaximumValidNumberOfDependencies
                let dependencies = g.Select(r => r.Dependency).ToArray()
                select new SingleResponsibilityViolationDiagnosticResult(
                    serviceType: g.Key.registration.ServiceType,
                    description: BuildRelationshipDescription(g.Key.ImplementationType, dependencies.Length),
                    implementationType: g.Key.ImplementationType,
                    dependencies: dependencies))
                .ToArray();           
        }
        
        private static bool IsAnalyzableRegistration(InstanceProducer registration)
        {
            // We can't analyze collections, because this would lead to false positives when decorators are
            // applied to the collection. For a decorator, each collection element it decorates is a 
            // dependency, which will make it look as if the decorator has too many dependencies. Since the
            // container will delegate the creation of those elements back to the container, those elements
            // would by them selve still get analyzed, so the only thing we'd miss here is the decorator.
            if (!registration.ServiceType.IsGenericType)
            {
                return true;
            }

            return registration.ServiceType.GetGenericTypeDefinition() != typeof(IEnumerable<>);
        }

        private static string BuildRelationshipDescription(Type implementationType, int numberOfDependencies)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0} has {1} dependencies which might indicate a SRP violation.",
                Helpers.ToFriendlyName(implementationType),
                numberOfDependencies);
        }

        private static string ViolationPlural(int count)
        {
            return count == 1 ? "violation" : "violations";
        }
    }
}