using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImportAppFunctions
{
    public class AzureMapAddress
    {
        [JsonProperty(PropertyName = "buildingNumber")]
        public string BuildingNumber { get; set; }
        //
        // Résumé :
        //     An address component which represents the name of a geographic area or locality
        //     that groups a number of addressable objects for addressing purposes, without
        //     being an administrative unit. This field is used to build the `freeformAddress`
        //     property.
        [JsonProperty(PropertyName = "localName")]
        public string LocalName { get; set; }
        //
        // Résumé :
        //     The full name of a first level of country administrative hierarchy. This field
        //     appears only in case countrySubdivision is presented in an abbreviated form.
        //     Only supported for USA, Canada, and Great Britain.
        [JsonProperty(PropertyName = "countrySubdivisionName")]
        public string CountrySubdivisionName { get; set; }
        //
        // Résumé :
        //     An address line formatted according to the formatting rules of a Result's country
        //     of origin, or in the case of a country, its full country name.
        [JsonProperty(PropertyName = "freeformAddress")]
        public string FreeformAddress { get; set; }
        //
        // Résumé :
        //     Country name.
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
        //
        // Résumé :
        //     Country (Note: This is a two-letter code, not a country name.).
        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }
        //
        // Résumé :
        //     Extended postal code (availability is dependent on the region).
        [JsonProperty(PropertyName = "extendedPostalCode")]
        public string ExtendedPostalCode { get; set; }
        //
        // Résumé :
        //     Postal Code / Zip Code.
        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }
        //
        // Résumé :
        //     State or Province.
        [JsonProperty(PropertyName = "countrySubdivision")]
        public string CountrySubdivision { get; set; }
        //
        // Résumé :
        //     ISO alpha-3 country code.
        [JsonProperty(PropertyName = "countryCodeISO3")]
        public string CountryCodeIso3 { get; set; }
        //
        // Résumé :
        //     County.
        [JsonProperty(PropertyName = "countrySecondarySubdivision")]
        public string CountrySecondarySubdivision { get; set; }
        //
        // Résumé :
        //     Sub / Super City.
        [JsonProperty(PropertyName = "municipalitySubdivision")]
        public string MunicipalitySubdivision { get; set; }
        //
        // Résumé :
        //     City / Town.
        [JsonProperty(PropertyName = "municipality")]
        public string Municipality { get; set; }
        //
        // Résumé :
        //     The street name and number.
        [JsonProperty(PropertyName = "streetNameAndNumber")]
        public string StreetNameAndNumber { get; set; }
        //
        // Résumé :
        //     The street name.
        [JsonProperty(PropertyName = "streetName")]
        public string StreetName { get; set; }
        //
        // Résumé :
        //     The codes used to unambiguously identify the street.
        //public IReadOnlyList<string> RouteNumbers { get; set; }
        //
        // Résumé :
        //     The building number on the street.
        [JsonProperty(PropertyName = "streetNumber")]
        public string StreetNumber { get; set; }
        //
        // Résumé :
        //     The name of the street being crossed.
        [JsonProperty(PropertyName = "crossStreet")]
        public string CrossStreet { get; set; }
        //
        // Résumé :
        //     The street name. DEPRECATED, use streetName instead.
        [JsonProperty(PropertyName = "street")]
        public string Street { get; set; }
        //
        // Résumé :
        //     Named Area.
        [JsonProperty(PropertyName = "countryTertiarySubdivision")]
        public string CountryTertiarySubdivision { get; }
    }


}
