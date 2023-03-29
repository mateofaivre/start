using Start.Core.Entities;
using Start.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Start.Core.Readers
{
    /// <summary>
    /// Lecteur de fichiers kml
    /// </summary>
    public class KmlReader
    {
        private static XNamespace ns = "http://www.opengis.net/kml/2.2";
        /// <summary>
        /// Extrait les oeuvres contenues dans un kml
        /// </summary>
        /// <param name="kmlItem"></param>
        /// <returns></returns>
        public static IEnumerable<Oeuvre> Read(Stream kmlItem)
        {
            XNamespace ns = "http://www.opengis.net/kml/2.2";
            var doc = XDocument.Load(kmlItem);
            var query = doc.Root
               .Element(ns + "Document")
               .Elements(ns + "Placemark")
               .Select(x => new OeuvreKml
               {
                   Date = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "date")?.Value,
                   Url = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "url")?.Value,
                   Rue = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "rue")?.Value,
                   Ville = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "ville")?.Value,
                   Type = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "type")?.Value,
                   Artiste = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "artiste")?.Value,
                   Information = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "information")?.Value,
                   Pseudo = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "pseudo")?.Value,
                   Marqueur = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "marqueur")?.Value,
                   Point = x.Element(ns + "Point").Element(ns + "coordinates")?.Value,
               });

            return query.Select(kml => ConvertToEntity(kml)).ToList();
        }

        /// <summary>
        /// Extrait les oeuvres contenues dans un kml
        /// </summary>
        /// <param name="kmlItem"></param>
        /// <returns></returns>
        public static IEnumerable<Partenaire> ReadKmlPartenaire(Stream kmlItem)
        {
            var doc = XDocument.Load(kmlItem);
            var query = doc.Root
               .Element(ns + "Document")
               .Elements(ns + "Placemark")
               .Select(x => new PartenaireKml
               {
                   Date = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "date")?.Value,
                   Url = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "url")?.Value,
                   Rue = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "rue")?.Value,
                   Ville = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "ville")?.Value,
                   Type = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "type")?.Value,
                   CodePostal = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "cp")?.Value,
                   Information = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "information")?.Value,
                   Nom = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "nom")?.Value,
                   SiteWeb = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "siteweb")?.Value,
                   Telephone = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "tel")?.Value,
                   Marqueur = x.Element(ns + "ExtendedData")
                           .Elements(ns + "Data")
                           .FirstOrDefault(d => d.Attribute("name").Value == "marqueur")?.Value,
                   Point = x.Element(ns + "Point").Element(ns + "coordinates")?.Value,
               });

            return query.Select(kml => ConvertToEntity(kml)).ToList();
        }

        public static XDocument CreateKml(IEnumerable<OeuvreGeoJson> oeuvres)
        {
            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));
            var kml = new XElement(ns + "kml", 
                new XElement("Document"));
            doc.Add(kml);

            foreach(var oeuvre in oeuvres)
            {
                var placemark = new XElement("Placemark", 
                    new XElement("ExtendedData"));

                placemark.Add(KmlData("date", oeuvre.Properties.DateCreationFormatted));
                placemark.Add(KmlData("url" ,oeuvre.Properties.ImageUrl));
                placemark.Add(KmlData("adresse", oeuvre.Properties.Adresse));
                placemark.Add(KmlData("rue", oeuvre.Properties.Rue));
                placemark.Add(KmlData("ville",oeuvre.Properties.Ville));
                placemark.Add(KmlData("type", oeuvre.Properties.TypeOeuvre));
                placemark.Add(KmlData("artiste", oeuvre.Properties.Artiste));
                placemark.Add(KmlData("information", oeuvre.Properties.Informations));
                placemark.Add(KmlData("pseudo", oeuvre.Properties.UtilisateurPseudo));
                placemark.Add(KmlData("marqueur", oeuvre.Properties.Marqueur));

                var coordinatates = KmlData("coordinates", $"{oeuvre.Geometry.Position.Longitude.ToString("0.000000", CultureInfo.InvariantCulture)}, {oeuvre.Geometry.Position.Latitude.ToString("0.000000", CultureInfo.InvariantCulture)}");
                placemark.Add(new XElement("Point", coordinatates));

                kml.Add(placemark);
            }

            return doc;
        }

        private static XElement KmlData(string name, string value)
        {
            return  new XElement("Data", new XAttribute("name", name),
                    new XElement("Value", value));
        }

        /// <summary>
        /// Convertit une oeuvre au format kml en une entité
        /// </summary>
        /// <param name="kml"></param>
        /// <returns></returns>
        private static Oeuvre ConvertToEntity(OeuvreKml kml)
        {
            var newOeuvre = new Oeuvre();
            newOeuvre.PartitionKey = OeuvrePartitionKeys.Item;
            newOeuvre.Marqueur = kml.Marqueur;

            if (!string.IsNullOrEmpty(kml.Date))
            {
                newOeuvre.DateCreation = DateTime.ParseExact(kml.Date, "dd/MM/yyyy", null);
            }

            newOeuvre.ImageUrl = string.IsNullOrEmpty(kml.Url) ? string.Empty : kml.Url.Replace("œ", "oe");
            if (string.IsNullOrEmpty(newOeuvre.ImageUrl))
            {
                newOeuvre.ImageUrl = kml.Marqueur;
            }

            if (newOeuvre.ImageUrl.StartsWith("http"))
            {
                //newOeuvre.ImageUrl = newOeuvre.ImageUrl.Replace("http", "https");
            }


            var location = kml.Point.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(p => ParserHelper.ParseDouble(p)).ToArray();
            /*En kml un point est sauvegarder [Long, Lat]*/
            newOeuvre.Location = new Microsoft.Azure.Cosmos.Spatial.Point(location[1], location[0]);

            var adresseValues = new string[] { kml.Rue, kml.Ville };

            newOeuvre.Adresse = adresseValues.Any(s => !string.IsNullOrEmpty(s)) 
                    ? string.Join(",", adresseValues.Where(s => !string.IsNullOrEmpty(s))) 
                    : string.Empty;
            newOeuvre.TypeOeuvre = kml.Type;
            newOeuvre.Artiste = kml.Artiste;
            newOeuvre.Informations = kml.Information;
            newOeuvre.UtilisateurPseudo = kml.Pseudo;

            return newOeuvre;
        }

        /// <summary>
        /// Convertit une oeuvre au format kml en une entité
        /// </summary>
        /// <param name="kml"></param>
        /// <returns></returns>
        private static Partenaire ConvertToEntity(PartenaireKml kml)
        {
            var newPartenaire = new Partenaire();
            newPartenaire.PartitionKey = PartenairePartitionKeys.Item;
            newPartenaire.Marqueur = kml.Marqueur;

            if (!string.IsNullOrEmpty(kml.Date))
            {
                newPartenaire.Date = DateTime.ParseExact(kml.Date, "dd/MM/yyyy", null);
            }

            newPartenaire.ImageUrl = string.IsNullOrEmpty(kml.Url) ? string.Empty : kml.Url.Replace("œ", "oe");
            if (string.IsNullOrEmpty(newPartenaire.ImageUrl))
            {
                newPartenaire.ImageUrl = kml.Marqueur;
            }


            var location = kml.Point.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(p => ParserHelper.ParseDouble(p)).ToArray();
            /*En kml un point est sauvegarder [Long, Lat]*/
            newPartenaire.Location = new Microsoft.Azure.Cosmos.Spatial.Point(location[1], location[0]);

            var adresseValues = new string[] { kml.Rue, kml.Ville };

            newPartenaire.Adresse = adresseValues.Any(s => !string.IsNullOrEmpty(s))
                    ? string.Join(",", adresseValues.Where(s => !string.IsNullOrEmpty(s)))
                    : string.Empty;
            newPartenaire.Rue = kml.Rue;
            newPartenaire.Ville = kml.Ville;
            newPartenaire.TypePartenaire = kml.Type;
            newPartenaire.CodePostal = kml.CodePostal;
            newPartenaire.Informations = kml.Information;
            newPartenaire.Nom = kml.Nom;
            newPartenaire.SiteWeb = kml.SiteWeb;
            newPartenaire.Telephone = kml.Telephone;

            return newPartenaire;
        }
    }
}
