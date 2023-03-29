using Start.Core.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Start.Core.Readers
{
    /// <summary>
    /// Définit le système de lecture des fichiers csv de good barber
    /// </summary>
    public class GoodBarberReader
    {
        /// <summary>
        /// Lit un fichier csv contenant des oeuvres
        /// </summary>
        /// <param name="filePath">Chemin d'accès au fichier</param>
        /// <returns>La liste des oeuvres contenues dans le fichier</returns>
        public static IEnumerable<Oeuvre> ReadCsvOeuvre(string filePath)
        {
            var oeuvreItems = new List<Oeuvre>();
            using (var reader = new StreamReader(filePath, Encoding.GetEncoding("ISO-8859-1")))
            {
                oeuvreItems = Read(reader);
            }

            return oeuvreItems;
        }

        /// <summary>
        /// Lit un fichier csv contenant des oeuvres
        /// </summary>
        /// <param name="csvStream">Le flux du fichier</param>
        /// <returns>La liste des oeuvres contenues dans le fichier</returns>
        public static IEnumerable<Oeuvre> ReadCsvOeuvre(Stream csvStream)
        {
            var oeuvreItems = new List<Oeuvre>();
            using (var reader = new StreamReader(csvStream, Encoding.GetEncoding("ISO-8859-1")))
            {
                oeuvreItems = Read(reader);
            }

            return oeuvreItems;
        }

        /// <summary>
        /// Parcours le flux du fichier et le traduit en une liste d'oeuvres
        /// </summary>
        /// <param name="streamReader">Le lecteur du flux du fichier csv</param>
        /// <returns>La liste des oeuvres contenues dans le flux</returns>
        private static List<Oeuvre> Read(StreamReader streamReader)
        {
            var oeuvreItems = new List<Oeuvre>();
            var line = string.Empty;
            int nbLine = 0;

            while (true)
            {
                try
                {
                    nbLine++;
                    line = streamReader.ReadLine();

                    if (string.IsNullOrEmpty(line))
                    {
                        break;
                    }

                    var columns = line.Split(";");

                    if (columns[0] == "date")
                    {
                        continue;
                    }
                    else if (columns.All(c => string.IsNullOrEmpty(c)))
                    {
                        break;
                    }


                    var oeuvre = ConvertToEntity(columns);
                    oeuvreItems.Add(oeuvre);

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur de lecture csv de la ligne N°{nbLine} {line}");
                    System.Diagnostics.Debug.WriteLine($"Erreur {ex.Message}");
                }
            }

            return oeuvreItems;
        }

        /// <summary>
        /// Ecrit un fichier csv avec une liste d'oeuvres
        /// </summary>
        /// <param name="oeuvres">Les oeuvres à écrire dans le fichier</param>
        /// <param name="filePath">Le chemin d'accès au fichier</param>
        public static void WriteCsvOeuvre(IEnumerable<Oeuvre> oeuvres, string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.GetEncoding("ISO-8859-1")))
            {
                writer.WriteLine("date;url;adresse;rue;ville;type;artiste;information;pseudo;marqueur;latitude;longitude");
                foreach (var oeuvre in oeuvres)
                {
                    writer.WriteLine(ConvertToLine(oeuvre));
                }
            }
        }

        /// <summary>
        /// Convertit une ligne du csv en une entité oeuvre
        /// </summary>
        /// <param name="csvRow">La ligne à convertir</param>
        /// <returns>Une oeuvre</returns>
        private static Oeuvre ConvertToEntity(string[] csvRow)
        {
            var newOeuvre = new Oeuvre();
            newOeuvre.DateCreation = DateTime.ParseExact(csvRow[0], "MM/dd/yyyy HH:mm", null);
            newOeuvre.ImageUrl = csvRow[1];

            var data = csvRow[1].Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            newOeuvre.Marqueur = data[^1];

            newOeuvre.Adresse = csvRow[2];
            newOeuvre.TypeOeuvre = csvRow[3];
            newOeuvre.Artiste = csvRow[4];
            newOeuvre.Informations = csvRow[5];
            newOeuvre.UtilisateurPseudo = csvRow[6];
            newOeuvre.UtilisateurEmail = csvRow[7];
            newOeuvre.PartitionKey = OeuvrePartitionKeys.Item;

            return newOeuvre;
        }

        /// <summary>
        /// Convertit une oeuvre en ligne csv
        /// </summary>
        /// <param name="oeuvre">L'oeuvre</param>
        /// <returns>La ligne csv</returns>
        private static string ConvertToLine(Oeuvre oeuvre)
        {
            var line = new string[12];
            
            line[0] = oeuvre.DateCreation.ToString("MM/dd/yyyy HH:mm");
            line[1] = oeuvre.ImageUrl;
            line[2] = oeuvre.Adresse;
            var adresses = oeuvre.Adresse.Split(",");
            line[3] = adresses[0];
            line[4] = adresses[1];
            line[5] = oeuvre.TypeOeuvre;
            line[6] = oeuvre.Artiste;
            line[7] = oeuvre.Informations;
            line[8] = oeuvre.UtilisateurPseudo;
            line[9] = oeuvre.Marqueur;
            line[10] = oeuvre.Location?.Position?.Latitude.ToString()?.Replace(",", ".");
            line[11] = oeuvre.Location?.Position?.Longitude.ToString().Replace(",", ".");

            return string.Join(";", line);
        }
    }
}
