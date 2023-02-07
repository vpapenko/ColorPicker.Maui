namespace ColorPicker.Test.xUnit.MathClasses
{
    public class PolarPointTests
    {
        /// <summary>
        /// Test creates new PolarPoints from PointF values and verifies correctness
        ///
        /// [InlineData( in float x, in float y, out float radius, out float angle )]
        /// </summary>
        [Theory]
        [InlineData( 0, 0, 0, 0 )]
        [InlineData( 1, 1, 1.4142135623731, 0.785398163397448 )]
        [InlineData( 150, 150, 212.132034355964, 0.785398163397448 )]
        [InlineData( 25, 50, 55.9016994374947, 1.10714871779409 )]
        [InlineData( -25, 50, 55.9016994374947, 2.0344439357957 )]
        public void PolarPointTests_ShouldConstructValidValue( float x, float y, float expectedRadius, float expectedAngle )
        {
            // Arrange and Act
            var actual = new PolarPoint( new PointF( x, y ) );

            // Assert
            Assert.Equal( expectedRadius, actual.Radius );
            Assert.Equal( expectedAngle, actual.Angle );
        }

        /// <summary>
        /// Test PolarPoint ToString
        ///
        /// [InlineData( in float radius, in float angle, string expected )]
        /// </summary>
        [Theory]
        [InlineData( 0, 0, "Radius: 0; Angle: 0" )]
        [InlineData( 1.4142135623731, 0.785398163397448, "Radius: 1.4142135; Angle: 0.7853982" )]
        public void PolarPointTests_ShouldReturnStringValue( float radius, float angle, string expected )
        {
            // Arrange and Act
            var sut = new PolarPoint( radius, angle );

            var actual = sut.ToString();

            Assert.Equal( expected, actual );
        }
    }
}
