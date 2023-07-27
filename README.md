# Diplomka
Tu sú zverejnené súbory mojej diplomovej práce.
V prezentácii chýbajú dve videá pretože s nimi bol súbor príliš veľký na nahratie.
Chýbajúce videá sa ale nachádzajú v priečinku videá.

Cieľom práce je vytvoriť na platforme unity Game engine prostredie pre simuláciu modelu
autonómneho vozidla a jeho trénovanie pomocou NEAT algoritmu. Vozidlo úspešne
dokončí zadefinované jazdné scenáre iba na základe senzorov, bez zásahu z vonku.

Riadenie vozidla bude riešené pomocou neurónovej siete s plne prepojenou štruktúrou,
natrénovanou pomocou algoritmu neuroevolúcie. Vstupom budú dáta zo senzorov,
akcelerometer a radar. Neurónová sieť bude mať jednu skrytú vrstvu. Výstup bude
prírastok k cieľovej polohe natočenia volantu vozidla, hĺbky stlačenia plynového pedálu a
hĺbky stlačenia brzdného pedálu.

Testovacie scenáre úspešnosti natrénovaného vozidla budú prebiehať v modely
postaveného vo formáte ASAM OpenDRIVE. Tento formát opisuje geometriu ciest, pruhov
a objektov, ako sú značky na ceste a taktiež aj prvky pozdĺž ciest, ako sú signály. Ide
o bezplatný štandard čo sa používa ako základ pre autonómne vozidlá.

Použité súčasti:

Unity 2020.3.40f1	    https://unity.com/releases/editor/qa/lts-releases

UnitySharpNEAT	      https://github.com/flo-wolf/UnitySharpNEAT

PathTool		          https://github.com/SebLague/Path-Creator

ESMini		            https://github.com/esmini/esmini

RoadRunner		        https://www.mathworks.com/products/roadrunner.html

