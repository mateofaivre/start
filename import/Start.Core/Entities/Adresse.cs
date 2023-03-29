namespace Start.Core.Entities
{
    public class Adresse
    {
        private string _pays;
        private string _ville;
        private string _rue;
        private string _freeformAddress;

        public Adresse(string freeformAddress)
        {
            _freeformAddress = freeformAddress;
        }

        public Adresse(string pays, string ville, string rue)
        {
            _pays = pays;
            _ville = ville;
            _rue = rue;
        }

        public Adresse()
        {
        }

        public string Pays 
        {
            get { return _pays; }
            set { _pays = value; }
        }
        public string Ville
        {
            get { return _ville; }
            set { _ville = value; }
        }
        public string Rue
        {
            get { return _rue; }
            set { _rue = value; }
        }
        public string FreeformAddress
        {
            get { return _freeformAddress; }
            set { _freeformAddress= value; }
        }

        public bool IsComplete
        {
            get
            {
                return !string.IsNullOrEmpty(FreeformAddress)
                    || (!string.IsNullOrEmpty(Pays) && !string.IsNullOrEmpty(Ville) && !string.IsNullOrEmpty(Rue));
            }
        }

        public override string ToString()
        {
            var rue = Rue?.Replace(",", " ");
            rue = !string.IsNullOrEmpty(rue) ? rue + ", " : string.Empty;

            var ville = Ville?.Replace(",", " ");
            ville = !string.IsNullOrEmpty(ville) ? ville + ", " : string.Empty;

            var pays = Pays?.Replace(",", " ");
            var adresse = $"{rue}{ville}{pays}";

            if (!string.IsNullOrEmpty(adresse))
            {
                return adresse;
            }

            else if (!string.IsNullOrEmpty(FreeformAddress))
            {
                return FreeformAddress;
            }

            return string.Empty;
        }
    }
}
