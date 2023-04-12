public class UrlsConfig
{
    public string SampleApi1 { get; set; }

    public string SampleApi2 { get; set; }

    public string GrpcPerson {get; set; }

    public class PersonOperations
    {
        public static string GetPerson(int id) => $"/api/v1/person/{id}";
    }
    
}