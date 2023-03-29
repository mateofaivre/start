using Newtonsoft.Json;
using System;

namespace Start.Core.Entities
{
    public enum OeuvreStatus
    {
        Valid = 0,
        WaitingValidation = 1,
        Deleted = 2,
        NotApproved = 3,
    }

    public static class OeuvrePartitionKeys
    {
        public const string Item = "item";
        public const string Updating = "modification";
        public const string NotApproved = "refuse";
        public const string Archive = "archive";
    }

    public static class TypeOeuvres
    {
        public const string Graffiti = "Graffiti";
        public const string Collage = "Collage";
        public const string Fresque = "Fresque";
        public const string Pochoir = "Pochoir";
        public const string Mosaique = "Mosaique";
        public const string Autre = "Autre";
    }

    /// <summary>
    /// Définit les propriétés d'une oeuvre sérialisable en json
    /// </summary>
    public class Oeuvre : Entity
    {
        [JsonProperty(PropertyName = "id")]
        public string Marqueur { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime DateCreation { get; set; }

        [JsonProperty(PropertyName = "dateModification")]
        public DateTime? DateModification { get; set; }

        [JsonProperty(PropertyName = "dateApprobation")]
        public DateTime? DateApprobation { get; set; }

        [JsonProperty(PropertyName = "datePhotoModification")]
        public DateTime? DatePhotoModification { get; set; }

        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "imageBrUrl")]
        public string ImageBrUrl { get; set; }

        [JsonProperty(PropertyName = "imageThumbnailUrl")]
        public string ImageThumbnailUrl { get; set; }

        [JsonProperty(PropertyName = "imageName")]
        public string ImageName { get; set; }

        [JsonProperty(PropertyName = "imageBase64")]
        public string ImageBase64 { get; set; }

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

        [JsonProperty(PropertyName = "location")]
        public Microsoft.Azure.Cosmos.Spatial.Point Location { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        /// <summary>
        /// Sérialise l'oeuvre en Json
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Keep old properties values if new values are empty
        /// </summary>
        /// <param name="oldOeuvre">Old values</param>
        /// <param name="newOeuvre">New values</param>
        public void UpdateValues(Oeuvre oldOeuvre, bool updatePhoto)
        {
            Marqueur = oldOeuvre.Marqueur;
            DateCreation = oldOeuvre.DateCreation;
            DateModification = DateTime.Now;
            DatePhotoModification = updatePhoto
                ? DateModification
                : oldOeuvre.DatePhotoModification;

            if (!updatePhoto)
            {
                IsLandscape = oldOeuvre.IsLandscape;
            }

            if (string.IsNullOrEmpty(Adresse))
            {
                Adresse = oldOeuvre.Adresse;
            }

            if (string.IsNullOrEmpty(Rue))
            {
                Rue = oldOeuvre.Rue;
            }

            if (string.IsNullOrEmpty(Ville))
            {
                Ville = oldOeuvre.Ville;
            }

            if (string.IsNullOrEmpty(Pays))
            {
                Pays = oldOeuvre.Pays;
            }

            if (string.IsNullOrEmpty(Artiste))
            {
                Artiste = oldOeuvre.Artiste;
            }

            if (string.IsNullOrEmpty(ImageUrl))
            {
                ImageUrl = oldOeuvre.ImageUrl;
            }

            if (string.IsNullOrEmpty(ImageBrUrl))
            {
                ImageBrUrl = oldOeuvre.ImageBrUrl;
            }

            if (string.IsNullOrEmpty(ImageThumbnailUrl))
            {
                ImageThumbnailUrl = oldOeuvre.ImageThumbnailUrl;
            }

            if (string.IsNullOrEmpty(Informations))
            {
                Informations = oldOeuvre.Informations;
            }

            if (string.IsNullOrEmpty(TypeOeuvre))
            {
                TypeOeuvre = oldOeuvre.TypeOeuvre;
            }

            UtilisateurIdModification = UtilisateurId;
            UtilisateurId = oldOeuvre.UtilisateurId;

            UtilisateurEmailModification = UtilisateurEmail;
            UtilisateurEmail = oldOeuvre.UtilisateurEmail;

            UtilisateurPseudoModification = UtilisateurPseudo;
            UtilisateurPseudo = oldOeuvre.UtilisateurPseudo;
        }

        public override string GetId()
        {
            return Marqueur;
        }
    }

}
