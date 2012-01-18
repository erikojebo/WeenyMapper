namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class UserWithExtraProperties : User
    {
        private string _privateField;
        private string _publicField;
        private static string _privateStaticField;
        private static string _publicStaticField;

        public UserWithExtraProperties()
        {
            _privateField = "Private field value";
            _publicField = "Public field value";
            _privateStaticField = "Private static field value";
            _publicStaticField = "Public static field value";

            PublicExtraProperty = "Public extra property value";
            PrivateProperty = 1;
            StaticProperty = 1;
        }

        private int PrivateProperty { get; set; }
        public static int StaticProperty { get; set; }
        public string PublicExtraProperty { get; set; }

        public string GetPrivateField()
        {
            return _privateField;
        }

        public string GetPrivateStaticField()
        {
            return _privateStaticField;
        }
        
        public string GetPublicField()
        {
            return _publicField;
        }

        public string GetPublicStaticField()
        {
            return _publicStaticField;
        }
    }
}