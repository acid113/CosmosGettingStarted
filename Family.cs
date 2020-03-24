using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


namespace CosmosGettingStartedTutorial
{
    public class Family
    {
        /* NOTE: "id" property is required when adding items to containers
         * - System.Text.Json's [JsonPropertyName("id")] does not work with azure-cosmos-dotnet-v3
         * - Maybe this will be fixed in v4 (still in preview version)
         */
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string LastName { get; set; }
        public List<Parent> Parents { get; set; }
        public List<Child> Children { get; set; }
        public Address Address { get; set; }
        public bool IsRegistered { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class Parent
    {
        public string FamilyName { get; set; }
        public string FirstName { get; set; }
    }

    public class Child
    {
        public string FamilyName { get; set; }
        public string FirstName { get; set; }
        public string Gender { get; set; }
        public int Grade { get; set; }
        public List<Pet> Pets { get; set; }
    }

    public class Pet
    {
        public string GivenName { get; set; }
    }

    public class Address
    {
        public string State { get; set; }
        public string County { get; set; }
        public string City { get; set; }
    }
}
