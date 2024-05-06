namespace AppBookify.Helpers
{
    public class HelperTools
    {
        public static string GenerateSalt()
        {
            Random random = new Random();
            string salt = "";
            for (int i = 1; i <= 50; i++)
            {
                int aleatorio = random.Next(1, 255);
                char letra = Convert.ToChar(aleatorio);
                salt += letra;
            }
            return salt;
        }

        public static bool CompareArrays(byte[] a, byte[] b)
        {
            bool iguales = true;
            if (a.Length != b.Length)
            {
                iguales = false;
            }
            else
            {
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i].Equals(b[i]) == false)
                    {
                        iguales = false;
                        break;
                    }
                }
            }
            return iguales;
        }

        public static string GenerateTokenMail()
        {

            return Guid.NewGuid().ToString();
            //Random random = new Random();
            //string token = "";
            //for (int i = 1; i <= 20; i++)
            //{
            //    //SIN CARACTERES EXTRAÑOS 65 - 122
            //    int aleatorio = random.Next(65, 122);
            //    char letra = Convert.ToChar(aleatorio);
            //    token += letra;
            //}
            //return token;
        }
    }
}
