function loadDetailPartenaire(feature, e) {
    $("#feature-img-gallerie").attr('src', 'images/nopicture.png');
    $(".button-navigate").attr('href', '');
    $(".adresse-rue").html('rue inconnue');
    $(".adresse-ville").html('Ville inconnu');
    $(".date-creation").html('Date de création non renseignée');
    $(".type").html('Type inconnu');
    $(".artist-name").html('Non renseigné(s)');
    $(".publisher-name").html('Inconnu');
    $("#information-gallerie").html('Aucune information à ce jour');
    $(".button-navigate").html("distance indéterminée");
    $(".zoom-img").attr('href', '');

    var data = feature.properties;
    console.log('gallerie', data);
    document.getElementById("feature-img-gallerie").style.backgroundImage = "url('" + data.url + "')";

    $(".button-navigate").attr('href', "https://www.google.com/maps/dir/?api=1&destination=" + e.latlng.lat + "," + e.latlng.lng + "&travelmode=walking&dir_action=navigate");

    if (data.rue && data.ville) {
        $(".adresse-rue").html(data.rue);
        $(".adresse-ville").html(data.ville);
    }
    else {
        var geocodeService = L.esri.Geocoding.geocodeService();
        geocodeService.reverse().latlng(e.latlng).run(function (error, result) {
            if (!error && result) {
                $(".adresse-ville").html(result.address.City);
                $(".adresse-rue").html(result.address.Address);
            }
        });

    }

    $(".type").html(data.typePartenaire);

    if (data.nom) {
        $(".lieu-name").html(data.nom);
        $(".lieu-name").click(function () {
            if (data.goodbarber) {
                window.location.href = data.goodbarber;
            }
        });
    }
    else if (!data.nom || data.nom === '' || data.nom === null || data.nom === ' ') {
        $(".lieu-name").html('Non renseigné(s)');
    }


    if (data.informations === '' || data.informations === null || data.informations === ' ') {
        $("#information-gallerie").html('Aucune information à ce jour');
        $(".information-complementaire").css('visibility', 'hidden');
    }
    else {
        $("#information-gallerie").html(data.information);
        $(".information-complementaire").css('visibility', 'visible');
    }


    if (e && _markerPosition) {
        $(".span-navigate").html(calcDistance(e) + " km");
    }
    else {
        $(".span-navigate").html("Y aller");
    }

    $("#telgallerie").attr('href', 'tel: ' + data.telephone);
    $("#telgallerie").html(data.telephone);

   // $(".site-gallerie").html('Site web');
    $("#siteweb").click(function () {
        window.location.href = data.siteWeb;
    });
}