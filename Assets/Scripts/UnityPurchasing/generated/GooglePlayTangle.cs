// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("JhvGqGmVisjAuqeqLmf7Zo0o5Dt8B/94/AH0GahvbgWhwlo6Gem4fPHjonj5cgvrHwnM3RKxkyH/my0124XfnyNzQfBy+ygHgC90mlPtA5rfXFJdbd9cV1/fXFxd+2Ca+4X0xwnWwi/v6QqL7ys9maNRhOi4KLh0BT3C5sucmWwWRzd7fxKCSyvdAHkuGb1IQOjJDSyagJW2jYj11863aHmdqC20Oijasnr5HsNF7D7A3O99aw207u/Hie7pXryqAMt2UOEz5vKyfkEmnMR0sbGumeZnFUwQNDgjI2YYbwgSfQJQGihfcNhHa0UEie3ebd9cf21QW1R32xXbqlBcXFxYXV58RGgMlsnnLS8N8m4cDLcNuXfCRt9IFqlldK7oOl9eXF1c");
        private static int[] order = new int[] { 5,3,2,9,9,7,12,13,9,11,10,11,13,13,14 };
        private static int key = 93;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
