using Newtonsoft.Json;

namespace AppBookify.Extensions
{
    public static class SessionExtension
    {
        //public static void SetObject
        //    (this ISession session, string key, object value)
        //{
        //    string data = JsonConvert.SerializeObject(value);
        //    session.SetString(key, data);
        //}

        //public static T GetObject<T>
        //    (this ISession session, string key)
        //{
        //    string data = session.GetString(key);
        //    if (data == null)
        //        return default(T);
        //    return JsonConvert.DeserializeObject<T>(data);
        //}

        //SESSION CON CLAVE ÚNICA (id de usuario) PARA GUARDAR EN CACHÉ
        public static void SetObject(this ISession session, string key, int userId, object value)
        {
            string data = JsonConvert.SerializeObject(value);
            session.SetString($"{key}_{userId}", data);
        }

        public static T GetObject<T>(this ISession session, string key, int userId)
        {
            string data = session.GetString($"{key}_{userId}");
            if (data == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
