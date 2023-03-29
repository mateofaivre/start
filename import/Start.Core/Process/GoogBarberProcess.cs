using Start.Core.Readers;
using Start.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Start.Core.Process
{
    public class GoogBarberProcess
    {
        private const string ftpImageRoot = "http://start-prod.fr/carte/images/oeuvres/";

        public async Task Execute(string filePath)
        {
            try
            {
               
                var oeuvres = GoodBarberReader.ReadCsvOeuvre(filePath);
                var errors = new List<Entities.Oeuvre>();

                using (var geoService = new GeolocalisationService())
                {
                    var nbOeuvres = oeuvres.Count();
                    Console.WriteLine($"{nbOeuvres} oeuvres à géolocaliser");
                    var imageExifService = new ImageExifService();

                    foreach (var oeuvre in oeuvres)
                    {
                        try
                        {

                            oeuvre.ImageUrl = ftpImageRoot + oeuvre.Marqueur;

                            if (!string.IsNullOrEmpty(oeuvre.Adresse) && oeuvre.Location == null)
                            {
                                Console.WriteLine($"Récupération de la position pour l'adresse suivante {oeuvre.Adresse}");
                                oeuvre.Location = await geoService.GetGeolocationAsync(oeuvre.Adresse);

                                var position = oeuvre.Location;
                                var longitude = position.Position.Longitude.ToString().Replace(",", ".");
                                var latitude = position.Position.Latitude.ToString().Replace(",", ".");

                                Console.WriteLine($"Position obtenue : Lat:{latitude} - Long:{longitude}");
                            }
                            else if (oeuvre.Location != null && string.IsNullOrEmpty(oeuvre.Adresse))
                            {
                                var position = oeuvre.Location;
                                var longitude = position.Position.Longitude.ToString().Replace(",", ".");
                                var latitude = position.Position.Latitude.ToString().Replace(",", ".");

                                Console.WriteLine($"Récupération de l'adresse pour la position suivante Lat:{latitude} - Long:{longitude}");
                                oeuvre.Adresse = await geoService.GetAddress(longitude, latitude);
                                Console.WriteLine($"Adresse obtenue : {oeuvre.Adresse}");
                            }
                            else
                            {
                                Console.WriteLine($"Récupération de la position en fonction des métadatas");
                                oeuvre.Location = await imageExifService.GetOeuvreGeolocalisation(oeuvre);
                                Console.WriteLine($"Position obtenue : {oeuvre.Location}");

                            }
                        }
                        catch
                        {
                            errors.Add(oeuvre);
                        }

                        Console.WriteLine($"{--nbOeuvres} oeuvres à géolocaliser");
                    }
                }

                var fo = new FileInfo(filePath);
                GoodBarberReader.WriteCsvOeuvre(oeuvres,Path.Combine(fo.Directory.FullName, "goodbarber-geoloc.csv"));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
    }
}
