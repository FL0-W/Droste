
# La Droste

La Droste est un nouveau projet de livraison par drone qui vise à effectuer des livraisons plus rapides, en particulier les livraisons urgentes, à tout moment du jour ou de la nuit. Le projet aura la forme d'une simulation utilisant la technologie avancée des drones pour améliorer la vitesse et l'efficacité des livraisons de colis, et fournira un service flexible et fiable aux clients.

Les drones utilisés dans le cadre du projet seront capables de transporter des colis pesant jusqu'à 2.5kg, et pourront voler dans une zone déterminée. Ils seront équipés d'un GPS ou d'une autre technologie de localisation, ainsi que d'une technologie d'évitement des obstacles pour garantir des vols sûrs et fluides.

## Quelques spécifications
Les spécifications suivantes sont issues du cahier des charges et sont triées par état de réalisation.

#### Fonctionnelles

- Mise en place de la physique du drone, respect des charges et des forces exercées sur l'appareil, y compris lorsque le drone est chargé
- Le drone doit être autonome une fois son objectif fixé.
	> **Actions réalisables**: MOVINGTOLOCATION, GOINGUPDOWN, GETTINGAPACKAGE, DROPPINGPACKAGE, GOBACKANDSHUTDOWN 

- Le drone peut enchainer les actions transmises par l'unité de contrôle.
- L'unité de contrôle peut attribuer un drone à une zone particulière.
	> **Note**: nécessite que le colis sois répertorié comme faisant partie de la zone en question
	
![Image zone](img/zone.PNG?raw=true)

#### A améliorer
- Gestion de la communication et des risques de collisions entre drones
	> **Note**: a été désactivée pour la simulation
- Interface de l'écran principal : seul le nombre de drones actifs et le nom du drone ciblé par la caméra de l'utilisateur apparaissent. L'historique des actions n'est disponible que via la console, mais une fenêtre de dialogue devait initialement afficher toutes les étapes et les points importants lors des déplacements des drones.

#### Non implémentées

- Action FREEZINGPOSITION : simuler un arrêt d'urgence, automatiquement ou manuellement pour un drone désigné depuis l'unité de contrôle.
- Prise en compte des phénomènes météorologiques
- Prise en compte de l'autonomie de la batterie en fonction du temps, des actions et de la distance parcourue par le drone
- Nouvelles catégories d'obstacles autres que les infrastructures (ex : oiseaux, drones non répertoriés, etc.)



## Lancer de la simulation


#### Comportement par défaut 

Les actions utilisateurs pour influencer la simulation initiale sont relativement restreinte. Les drones déjà en place ont déjà une liste d'actions prête : 

- S'initialiser (Power up)
- Se rendre à une hauteur de 15m (action: GOINGUPDOWN)
- Aller chercher le colis qui lui a été attribué dans la zone (action MOVINGTOLOCATION)
- Descendre au niveau du colis, le rattacher au drone, puis remonter à l'altitude initiale (action GETTINGAPACKAGE)
- Prendre la direction de la zone de destination du colis (action MOVINGTOLOCATION)
- Descendre, se détacher du colis et remonter à l'altitude initiale (action DROPPINGPACKAGE)
- SOIT retourner à sa position d'origine si aucun autre colis n'est en attente, SOIT répéter ces actions si l'unité centrale lui attribue un autre colis sur sa zone

> **Note** : certains drones ont été dirigés vers un objectif qui leur est inateignable. Si les délais avaient été plus longs, il aurait été intéressants de mettre un terme à leurs tentatives répétitives de passer à travaver un obstacle trop gros sans pouvoir le contourner,  ni passer au dessus.

#### Variables modifiables

Globalement, via l'interface Unity, l'utilisateur peut changer manuellement :

##### Unité de contrôle
- L'altitude à laquelle les drones vont s'élever en cliquant sur l'objet "*Control Unit*" et en modifiant la propriété "*Height To Fly*".
- Le lancement automatique des drones lors de l'exécution du programme en cochant/décochant la case "*Activate*".

![Image unit](img/unit.PNG?raw=true)

##### Colis

> **Note** : la liste des colis se trouve dans "Packages" 
- La destination du colis du colis (propriétés "*X Target*", "*Y Target*", "*Z Target*")
- La zone attribuée à ce colis via "*Zone Name*"
![Image colis](img/colis.PNG?raw=true)

##### Drone

> **Note** : la liste des drones se trouve dans "Drones" 

- La disponibilité d'un drone via la propriété "*Available*"
- La zone attribuée au drone via la propriété "*Zone Name*".

![Image drone](img/drone.PNG?raw=true)

##### Exemple d'utilisation

Il est tout a fait possible pour un utilisateur d'ajouter un nouveau drone et un nouveau colis via la liste des préfabs et de les faire interagir en leur attribuant un même nom de zone (ex : ZONETEST). Tant que les tags sont bien attribués aux objets concernés, l'unité de contrôle pourra lancer le nouveau drone et lui faire récupérer le colis pour l'amener à la destination souhaitée.








> *Projet réalisé par Florentin BALFOUONG & Benjamin ROOSEBROUCK*
> *Idée originale : Youssef Blaiha et Valentin Leonard Noske*


