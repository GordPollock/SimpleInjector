namespace SimpleInjector.CodeSamples.Tests.Unit
{
    public class CustomExpressionExtensionsTests
    {
        public void FailToCompile()
        {
            var c = new Container();

            var t = new TestDto {X = 5};
            c.RegisterExpression(_ => t);
        }
    }

    public class TestDto
    {
        public int X { get; set; }
    }
}
