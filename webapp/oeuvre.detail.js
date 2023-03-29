$(document).ready(function () {
    $(".close").css('display', 'none');
});

function loadDetailOeuvre(feature, e) {
    //Initialisation des données à afficher
    $(".button-navigate").attr('href', '');
    $(".button-navigate").html("distance indéterminée");
    $(".adresse-rue").html('rue inconnue');
    $(".adresse-ville").html('Ville inconnu');
    $(".date-creation").html('Date de création non renseignée');
    $(".type").html('Type inconnu');
    $(".artist-name").html('Non renseigné(s)');
    $(".publisher-name").html('Inconnu');
    $("#information").html('Aucune information à ce jour');

    //Récupération des données
    var data = feature.properties;

    console.log('Détails ', data);
    $("#modifier").css('display', 'none');
    $("#approuver").css('display', 'none');
    $("#refuser").css('display', 'none');
    $("#reintegrer").css('display', 'none');

    $(".update-info").css('visibility', 'hidden');

    $("#valider").css('display', 'none');
    $("#annuler").css('display', 'none');
    $('.btndelete').attr("style", "display: none !important");
    $("#status").css('display', 'none');
    $("#deleteStatus").css('display', 'none');

    $("#busy-save").css('display', 'none');

    //Gestion des boutons en fonction du user (identifié et admin)
    if (_userIsLogin) {

        //Validée
        if (data.status == 0) {
            $("#modifier").css('display', 'inline-block');


            if (_userIsLogin && !_isAdmin) {
                $("#status").css('display', 'inline-block');
                if (data.soon) {
                    $("#status").html('Future nouveauté');
                }
                else {
                    $("#status").html('');
                }
                //else {
                //    $("#status").html('Visible de tous');
                //}
            }
        }

        //Si administrateur
        if (_isAdmin) {

            //Nouvelle, validé, en attente
            if (data.status !== 2) {
                $('.btndelete').attr("style", "display: inline-block !important");
            }

            //Supprimée
            if (data.status == 2) {
                $("#deleteStatus").css('display', 'inline-block');
                $("#reintegrer").css('display', 'inline-block');
            }

            //En attente
            if (data.status == 1) {
                $("#approuver").css('display', 'inline-block');
                $("#modifier").css('display', 'inline-block');
                $("#refuser").css('display', 'inline-block');
            }
        }
        else {
            if (data.status == 1) {
                $("#modifier").css('display', 'inline-block');
            }

            $('.btndelete').attr("style", "display: none !important");
        }
    }

    //Donnée en attente de validation
    if (data.status == 1) {
        $("#status").css('display', 'inline-block');
        $("#status").html('En attente de validation');
    }


    //Image
    //document.getElementById("feature-img").style.backgroundImage = "url('data:image/jpeg;base64," + data.url + "')";
    var url = "./pas-de-photos.png";
    var hasPhoto = false;
    if (data.imageBrUrl) {
        url = data.imageBrUrl;
        hasPhoto = true;
    }

    document.getElementById("feature-img").style.backgroundImage = "url('" + url + "')";
    if (!hasPhoto || data.isLandscape) {
        $('.feature-img').removeClass("pic-portrait");
        $('.feature-img').addClass("pic");
    }
    else {
        $('.feature-img').removeClass("pic");
        $('.feature-img').addClass("pic-portrait");
    }
    

    //Artiste
    if (data.artiste) {
        $(".artist-name").html(data.artiste);
    }
    else {
        $(".artist-name").html('Non renseigné(s)');
    }

    //Type de l'oeuvre
    $(".type").html(data.typeOeuvre);

    //Adresse
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

    //Accès à la route pour s'y rendre
    $(".button-navigate").attr('href', "https://www.google.com/maps/dir/?api=1&destination=" + e.latlng.lat + "," + e.latlng.lng + "&travelmode=walking&dir_action=navigate");

    if (e && _markerPosition) {
        $(".span-navigate").html(calcDistance(e) + " km");
    }
    else {
        $(".span-navigate").html("Y aller");
    }

    //Pseudo de la personne ayant publié
    if (data.utlisisateurPseudo) {
        $(".publisher-name").html(data.utlisisateurPseudo);
    }
    else {
        $(".publisher-name").html('Inconnu');
    }

    //Date de publication
    var date = data.date;
    if (date) {
        $(".date-creation").html('le ' + date);
    }

    var dateModification = data.dateModification;
    if (dateModification) {

        $(".update-info").css('visibility', 'visible');
        //Pseudo de la personne ayant modifié
        if (data.utlisisateurPseudoModification) {
            $(".modifier-name").html(data.utlisisateurPseudoModification);
        }

        //Date de modification
        if (dateModification) {
            $(".date-modification").html('le ' + dateModification);
        }
    }


    //Informations complémentaires
    if (data.informations) {
        $("#information").html(data.informations);
        $(".information-complementaire").css('visibility', 'visible');
    }
    else {
        $(".information-complementaire").css('visibility', 'hidden');
    }

    //Modification des infos de l'oeuvre
    $("#modifier").click(function () {
        clearSideBar();
        showSideBarForm(feature, e);
    });

    //Demande d'approbation
    $("#approuver").click(function () {
        $("#modifier").css('display', 'none');
        $("#approuver").css('display', 'none');
        $("#refuser").css('display', 'none');
        $("#msgConfirmation").html('Approbation en cours ...');
        $("#busy-save").css('display', 'inline-block');

        approveOeuvre(data.id, true);
    });

    //Demande de refus d'un ajout ou d'une modification
    $("#refuser").click(function () {
        $("#modifier").css('display', 'none');
        $("#approuver").css('display', 'none');
        $("#refuser").css('display', 'none');
        $("#msgConfirmation").html('Refus en cours ...');
        $("#busy-save").css('display', 'inline-block');

        approveOeuvre(data.id, false);
    });

    //Click sur suppression
    $("#delete").click(function () {
        waitDelete = true;

        $('.btndelete').attr("style", "display: none !important");
        $("#msgConfirmation").css('display', 'inline-block');
        $("#msgConfirmation").html('Confirmez la suppression ?');

        $("#valider").css('display', 'inline-block');
        $("#annuler").css('display', 'inline-block');


        if (_sidebarFeature.properties.status == 1) {
            $("#approuver").css('display', 'none');
            $("#modifier").css('display', 'none');
            $("#refuser").css('display', 'none');
        }
        else {

            $("#modifier").css('display', 'none');
        }
    });

    //Click sur réintégration
    $("#reintegrer").click(function () {
        waitUnDelete = true;

        $("#reintegrer").css('display', 'none');

        $("#msgConfirmation").css('display', 'inline-block');
        $("#msgConfirmation").html('Confirmez la réintégration ?');

        $("#valider").css('display', 'inline-block');
        $("#annuler").css('display', 'inline-block');
    });

    //Confirmation de suppression ou de réintégration
    $("#valider").click(function () {
        $("#msgConfirmation").html('Traitement en cours ...');
        $("#valider").css('display', 'none');
        $("#annuler").css('display', 'none');
        $("#busy-save").css('display', 'inline-block');

        if (waitDelete) {
            deleteOeuvre(data.id);
        }
        else if (waitUnDelete) {
            unDeleteOeuvre(data.id)
        }
    });

    //Annulation de la demande de suppression ou de réintégration
    $("#annuler").click(function () {

        if (waitDelete || waitUnDelete) {
            $("#valider").css('display', 'none');
            $("#annuler").css('display', 'none');
            $("#msgConfirmation").css('display', 'none');


            //Nouvelle, validé, en attente
            if (data.status !== 2) {
                $('.btndelete').attr("style", "display: inline-block !important");
            }

            //Supprimée
            if (data.status == 2) {
                $("#deleteStatus").css('display', 'inline-block');
                $("#reintegrer").css('display', 'inline-block');
            }

            //En attente
            if (data.status == 1) {
                $("#approuver").css('display', 'inline-block');
                $("#modifier").css('display', 'inline-block');
                $("#refuser").css('display', 'inline-block');
                $("#status").css('display', 'inline-block');
            }

            if (data.status == 0) {
                $("#modifier").css('display', 'inline-block');
            }
        }
    });
}

function approveOeuvre(oeuvreId, approved) {

    event.preventDefault();

    $("#valider").prop("disabled", true);
    $("#annuler").prop("disabled", true);

    var sendInfo = {
        OeuvreId: oeuvreId,
        Approved: approved,
    };

    $.ajax({
        type: "POST",
        url: appConfig.ApproveOeuvreFunction,
        //url: "http://localhost:7071/api/ApproveOeuvreFunction",
        //url: "https://import-kmz.azurewebsites.net/api/ApproveOeuvreFunction?code=umJaKjMW3glSJa0s1cyfgwp9tgdL104mocXQRm7qp833KOxrCK2XeQ==",
        data: JSON.stringify(sendInfo),
        dataType: "json",
        processData: false,
        contentType: 'application/json',
        cache: false,
        timeout: 600000,
        success: function (data) {

            console.log("SUCCESS APPROVED : ", data);

            //searchByLayer();

            //if (sendInfo.Approved) {
            //    _sidebarFeature.properties = data;
            //    _sidebarFeature.properties.url = correctImageUrl(data.imageUrl);

            //    $("#status").css('display', 'none');
            //    $("#modifier").css('display', 'inline-block');
            //    $("#approuver").css('display', 'none');
            //    $("#valider").css('display', 'none');
            //    $("#annuler").css('display', 'none');
            //    $("#busy-save").css('display', 'none');
            //    $("#msgConfirmation").css('display', 'none');
            //}

            handleBack(false);
        },
        error: function (e) {

            console.log("ERROR : ", e);
        }
    });
}

function deleteOeuvre(oeuvreId) {

    event.preventDefault();
    waitDelete = false;

    $.ajax({
        type: "DELETE",
        url: appConfig.DeleteOeuvreFunction + "&oeuvreId=" + oeuvreId,
        //url: "http://localhost:7071/api/DeleteOeuvreFunction?oeuvreId=" + oeuvreId,
        //url: "https://import-kmz.azurewebsites.net/api/DeleteOeuvreFunction?code=orJGQa645FXhG54URf8INe0UNvI7Gaymf8ZMeWE743Ccxcf23u/Fhg==&oeuvreId=" + oeuvreId,
        processData: false,
        contentType: false,
        cache: false,
        timeout: 600000,
        success: function (data) {

            handleBack(false);
        },
        error: function (e) {

            console.log("ERROR : ", e);
        }
    });
}

function unDeleteOeuvre(oeuvreId) {

    event.preventDefault();
    waitUnDelete = false;


    var sendInfo = {
        OeuvreId: oeuvreId,
        Admin: _userEmail
    };

    $.ajax({
        type: "POST",
        url: appConfig.UnDeleteOeuvreFunction,
        //url: "http://localhost:7071/api/UnDeleteOeuvreFunction",
        //url: "https://import-kmz.azurewebsites.net/api/UnDeleteOeuvreFunction?code=rRKjjJXfAsaX4MIbQlMcjAzMRB7aWqWu0MsqT28Zxbz7p4JEvk9Xbw==",
        data: JSON.stringify(sendInfo),
        dataType: "json",
        processData: false,
        contentType: 'application/json',
        cache: false,
        timeout: 600000,
        success: function (data) {

            handleBack(false);
        },
        error: function (e) {

            console.log("ERROR : ", e);
        }
    });
}

