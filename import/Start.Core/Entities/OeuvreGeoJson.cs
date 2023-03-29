using Microsoft.Azure.Cosmos.Spatial;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Start.Core.Entities
{
    public class QueryResult<T>
    {
        public List<T> Data { get; set; }

        public int Count { get; set; }

        public double RequestUnit { get; set; }

        public QueryResult(List<T> data)
        {
            Data = data ?? new List<T>();
        }
    }

    public class FeatureDataResult<T>
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "features")]
        public IEnumerable<T> Features { get; set; }

        public FeatureDataResult(List<T> features)
        {
            Type = "FeatureCollection";
            Features = features;
        }
    }

    public class OeuvreQueryResult : QueryResult<OeuvreGeoJson>
    {
        [JsonProperty(PropertyName = "deleted")]
        public FeatureDataResult<OeuvreGeoJson> Deleted { get; set; }

        [JsonProperty(PropertyName = "new")]
        public FeatureDataResult<OeuvreGeoJson> New { get; set; }

        [JsonProperty(PropertyName = "waiting")]
        public FeatureDataResult<OeuvreGeoJson> Waiting { get; set; }

        [JsonProperty(PropertyName = "valid")]
        public FeatureDataResult<OeuvreGeoJson> Valid { get; set; }

        [JsonProperty(PropertyName = "approved")]
        public FeatureDataResult<OeuvreGeoJson> Approved { get; set; }

        public OeuvreQueryResult(List<OeuvreGeoJson> oeuvres) : base(oeuvres)
        {
            
        }
    }

    public class OeuvreGeoJson
    {
        [JsonProperty(PropertyName = "geometry")]
        public Point Geometry { get; set; }

        [JsonProperty(PropertyName = "properties")]
        public PropertiesOeuvreGeoJson Properties { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        public OeuvreGeoJson(Oeuvre oeuvre)
        {
            Type = "Feature";
            Geometry = oeuvre.Location;
            Properties = new PropertiesOeuvreGeoJson
            {
                Adresse = oeuvre.Adresse,
                Rue = oeuvre.Rue,
                Ville = oeuvre.Ville,
                Pays = oeuvre.Pays,
                Artiste = oeuvre.Artiste,
                DateCreation = oeuvre.DateCreation,
                DateCreationFormatted = oeuvre.DateCreation.ToString("dddd dd MMMM yyyy", CultureInfo.CreateSpecificCulture("fr-FR")),
                DateModification = oeuvre.DateModification,
                DateModificationFormatted = oeuvre.DateModification.HasValue 
                    ? oeuvre.DateModification.Value.ToString("dddd dd MMMM yyyy", CultureInfo.CreateSpecificCulture("fr-FR"))
                    : null,
                DateApprobation = oeuvre.DateApprobation,
                DateApprobationFormatted = oeuvre.DateApprobation.HasValue
                    ? oeuvre.DateApprobation.Value.ToString("dddd dd MMMM yyyy", CultureInfo.CreateSpecificCulture("fr-FR"))
                    : null,
                DatePhotoModification = oeuvre.DatePhotoModification,
                DatePhotoModificationFormatted = oeuvre.DatePhotoModification.HasValue
                    ? oeuvre.DatePhotoModification.Value.ToString("dddd dd MMMM yyyy", CultureInfo.CreateSpecificCulture("fr-FR"))
                    : null,
                IsLandscape = oeuvre.IsLandscape,
                ImageUrl = oeuvre.ImageUrl,
                ImageBrUrl = oeuvre.ImageBrUrl,
                ImageThumbnailUrl = oeuvre.ImageThumbnailUrl,
                Informations = oeuvre.Informations,
                Marqueur = oeuvre.Marqueur,
                TypeOeuvre = oeuvre.TypeOeuvre,
                UtilisateurId = oeuvre.UtilisateurId,
                UtilisateurEmail = oeuvre.UtilisateurEmail,
                UtilisateurPseudo = oeuvre.UtilisateurPseudo,
                UtilisateurIdModification = oeuvre.UtilisateurIdModification,
                UtilisateurEmailModification = oeuvre.UtilisateurEmailModification,
                UtilisateurPseudoModification = oeuvre.UtilisateurPseudoModification,
                Status = oeuvre.Status,
            };

            //System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(oeuvre.Location).ToString());
        }
    }

    public class PropertiesOeuvreGeoJson
    {
        [JsonProperty(PropertyName = "id")]
        public string Marqueur { get; set; }

        internal DateTime DateCreation { get; set; }

        [JsonProperty(PropertyName = "date")]
        public string DateCreationFormatted { get; set; }

        internal DateTime? DateModification { get; set; }

        [JsonProperty(PropertyName = "dateModification")]
        public string DateModificationFormatted { get; set; }

        internal DateTime? DateApprobation { get; set; }

        [JsonProperty(PropertyName = "dateApprobation")]
        public string DateApprobationFormatted { get; set; }

        internal DateTime? DatePhotoModification { get; set; }

        [JsonProperty(PropertyName = "datePhotoModification")]
        public string DatePhotoModificationFormatted { get; set; }

        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "imageBrUrl")]
        public string ImageBrUrl { get; set; }

        [JsonProperty(PropertyName = "imageThumbnailUrl")]
        public string ImageThumbnailUrl { get; set; }

        [JsonProperty(PropertyName = "imageBase64")]
        public string ImageBase64 { get; set; }

        [JsonProperty(PropertyName = "imageName")]
        public string ImageName { get; set; }

        [JsonProperty(PropertyName = "isLandscape")]
        public bool IsLandscape { get; set; }

        [JsonProperty(PropertyName = "adresse")]
        public string Adresse { get; set; }

        [JsonProperty(PropertyName = "rue")]
        public string Rue { get; set; }

        [JsonProperty(PropertyName = "ville")]
        public string Ville { get; set; }

        [JsonProperty(PropertyName = "pays")]
        public string Pays { get; set; }

        [JsonProperty(PropertyName = "typeOeuvre")]
        public string TypeOeuvre { get; set; }

        [JsonProperty(PropertyName = "artiste")]
        public string Artiste { get; set; }

        [JsonProperty(PropertyName = "informations")]
        public string Informations { get; set; }

        [JsonProperty(PropertyName = "utlisisateurId")]
        public string UtilisateurId { get; set; }

        [JsonProperty(PropertyName = "utlisisateurPseudo")]
        public string UtilisateurPseudo { get; set; }

        [JsonProperty(PropertyName = "utilisaeurEmail")]
        public string UtilisateurEmail { get; set; }

        [JsonProperty(PropertyName = "utlisisateurIdModification")]
        public string UtilisateurIdModification { get; set; }

        [JsonProperty(PropertyName = "utlisisateurPseudoModification")]
        public string UtilisateurPseudoModification { get; set; }

        [JsonProperty(PropertyName = "utilisaeurEmailModification")]
        public string UtilisateurEmailModification { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "soon")]
        public bool Soon { get; set; }
    }


    public class PartenaireQueryResult : QueryResult<PartenaireGeoJson>
    {
        [JsonProperty(PropertyName = "valid")]
        public FeatureDataResult<PartenaireGeoJson> Valid { get; set; }

        public PartenaireQueryResult(List<PartenaireGeoJson> oeuvres) : base(oeuvres)
        {
            if (Data.Any())
            {
                Valid = new FeatureDataResult<PartenaireGeoJson>(Data.ToList());
            }
        }
    }

    public class SearchGlobalQueryResult
    {
        [JsonProperty(PropertyName = "deleted")]
        public FeatureDataResult<OeuvreGeoJson> Deleted { get; set; }

        [JsonProperty(PropertyName = "new")]
        public FeatureDataResult<OeuvreGeoJson> New { get; set; }

        [JsonProperty(PropertyName = "waiting")]
        public FeatureDataResult<OeuvreGeoJson> Waiting { get; set; }

        [JsonProperty(PropertyName = "valid")]
        public FeatureDataResult<OeuvreGeoJson> Valid { get; set; }

        [JsonProperty(PropertyName = "approved")]
        public FeatureDataResult<OeuvreGeoJson> Approved { get; set; }

        [JsonProperty(PropertyName = "partenaire")]
        public FeatureDataResult<PartenaireGeoJson> Partenaire { get; set; }
    }

    public class PartenaireGeoJson
    {
        [JsonProperty(PropertyName = "geometry")]
        public Point Geometry { get; set; }

        [JsonProperty(PropertyName = "properties")]
        public PropertiesPartenaireGeoJson Properties { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        public PartenaireGeoJson(Partenaire partenaire)
        {
            Type = "Feature";
            Geometry = partenaire.Location;
            Properties = new PropertiesPartenaireGeoJson
            {
                Adresse = partenaire.Adresse,
                Rue = partenaire.Rue,
                Ville = partenaire.Ville,
                Nom = partenaire.Nom,
                Date = partenaire.Date,
                DateFormatted = partenaire.Date.ToString("dd, dddd MMMM yyyy", CultureInfo.CreateSpecificCulture("fr-FR")),
                DateModification = partenaire.DateModification,
                DateModificationFormatted = partenaire.DateModification.HasValue
                    ? partenaire.DateModification.Value.ToString("D(fr-FR)")
                    : null,
                ImageUrl = partenaire.ImageUrl,
                Informations = partenaire.Informations,
                Marqueur = partenaire.Marqueur,
                TypePartenaire = partenaire.TypePartenaire,
                SiteWeb = partenaire.SiteWeb,
                Telephone = partenaire.Telephone,
            };

            System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(partenaire.Location).ToString());
        }
    }

    public class PropertiesPartenaireGeoJson
    {
        [JsonProperty(PropertyName = "id")]
        public string Marqueur { get; set; }

        internal DateTime Date { get; set; }

        [JsonProperty(PropertyName = "date")]
        public string DateFormatted { get; set; }

        internal DateTime? DateModification { get; set; }

        [JsonProperty(PropertyName = "dateModification")]
        public string DateModificationFormatted { get; set; }

        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "adresse")]
        public string Adresse { get; set; }

        [JsonProperty(PropertyName = "rue")]
        public string Rue { get; set; }

        [JsonProperty(PropertyName = "ville")]
        public string Ville { get; set; }

        [JsonProperty(PropertyName = "typePartenaire")]
        public string TypePartenaire { get; set; }

        [JsonProperty(PropertyName = "nom")]
        public string Nom { get; set; }

        [JsonProperty(PropertyName = "informations")]
        public string Informations { get; set; }

        [JsonProperty(PropertyName = "telephone")]
        public string Telephone { get; set; }

        [JsonProperty(PropertyName = "siteWeb")]
        public string SiteWeb { get; set; }
    }
}
