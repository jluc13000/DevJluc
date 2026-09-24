using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Lifetime;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Poker.MainWindow;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;


namespace Poker
{

    public class MainHistorique
    {
        public string M1 { get; set; }
        public string M2 { get; set; }
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string T { get; set; }
        public string R { get; set; }
    }

    public class Carte
    {
        public int Valeur;
        public char Couleur;
        public bool EstMain;   // true = M1/M2, false = Board
    }

    public class Jeu
    {
        // 0 = Rien
        // 1 = Paire
        // 2 = Deux Paires
        // 3 = Brelan
        // 4 = Quinte
        // 5 = Couleur
        // 6 = Full
        // 7 = Carré
        // 8 = Quinte Flush
        public int Force = 0;

        // Valeur de la combinaison
        // Ex : As pour une couleur As haute
        // Ex : As pour un brelan d'As
        // Ex : 10 pour une quinte 10-J-Q-K-A
        public int Qualite = 0;

        // Nombre de cartes de ma main
        // participant à la combinaison
        public int Participation = 0;

        // Indique si la quinte est également une quinte flush
        public bool EstQuinteFlush = false;

        // Les 5 cartes de la quinte détectée
        public List<Carte> CartesQuinte = new List<Carte>();
    }

    public class Tirage
    {
        public bool Quinte;
        public bool Ventrale;
        public bool Couleur;
        public bool QuinteFlush;

        // 0, 1 ou 2 cartes de ma main
        public int Participation;

        // Tirage max ?
        public bool Max;

        // Carte la plus haute de ma main
        public int CarteHaute;

        // Dangers
        public bool DangerQuinte { get; set; }
        public bool DangerCouleur { get; set; }
        public bool DangerQuinteFlush { get; set; }
        public bool DangerCarre { get; set; }
        public bool DangerFull { get; set; }
    }

    // =========================================================
    // 🔥 SIGNATURE D'UN CHIFFRE
    // =========================================================
    public class Signature_Chiffre
    {
        public int Chiffre { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Couleur { get; set; }
    }

    // =========================================================
    // 🔥 CARTOGRAPHIE D'UNE POSITION
    // =========================================================
    public class Cartographie_Position
    {
        public string Nom { get; set; } = "";

        public List<Signature_Chiffre> Signatures { get; set; }
            = new List<Signature_Chiffre>();
    }

    // =========================================================
    // 🔥 CARTOGRAPHIE D'UNE LONGUEUR
    // =========================================================
    public class Cartographie_Longueur
    {
        public int NombreChiffres { get; set; }

        public int PixelX { get; set; }
        public int PixelY { get; set; }
        public int PixelCouleur { get; set; }

        public List<Cartographie_Position> Positions { get; set; }
            = new List<Cartographie_Position>();
    }

    // =========================================================
    // GOODGAME STUDIOS
    // =========================================================
    // IMPORTANT : Installer l'extension uBlock Origin.
    //
    // Ajouter le filtre suivant dans "Mes filtres" :
    //
    // poker.goodgamestudios.com###wrapper-gamebar
    //
    // Ce filtre supprime le bandeau supérieur et constitue
    // la géométrie de référence utilisée par toutes les
    // injections de pixels.
    // =========================================================

    public partial class MainWindow : Window
    {

        // =========================================================
        // 🔥 CARTOGRAPHIE DU POT
        // =========================================================
        private Dictionary<int, Cartographie_Longueur>
            CartographieDuPot = new Dictionary<int, Cartographie_Longueur>();

        // -------------------------------------
        // Temps laissé au joueur pour reprendre
        // le contrôle avant le clic automatique
        // Réglable par l'utilisateur
        // Valeur par défaut : 4000 ms
        // -------------------------------------
        private int DELAI_PRISE_CONTROLE = 4000;

        private int NIVEAU_MISE = -8;

        private int MEMO = 6;

        private int NbPreferences = 0;

        private Image[] ImagesMonAvatar;

        private bool bAttendrePosition = false;

        // =====================================
        // POSITION ET GAGNANT
        // =====================================
        private int MaPlace = 0;
        private int Gagnant = 0;

        // =========================================================
        // API WINDOWS SOURIS / CLAVIER
        // =========================================================
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(
            uint dwFlags,
            uint dx,
            uint dy,
            uint dwData,
            UIntPtr dwExtraInfo);

        // 🔥 CLAVIER
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        private const int VK_SPACE = 0x20;

        // =========================================================
        // 🔥 DÉPLACER LA SOURIS
        // =========================================================
        private void DeplacerSouris(int x, int y)
        {
            SetCursorPos(x, y);
        }


        private const string FichierHistoriqueTXT = @"F:\source\repos\Poker\Historique.txt";

        private const string FichierHistoriqueRTF = @"F:\source\repos\Poker\Historique.rtf";

        private const string DOSSIER_SONS = @"F:\source\repos\Poker\Sons\";

        // =========================================================
        // 🔥 PLAYSOUND WINDOWS
        // =========================================================
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern bool PlaySound(
            string pszSound,
            IntPtr hmod,
            uint fdwSound);

        private const uint SND_ASYNC = 0x0001;
        private const uint SND_FILENAME = 0x00020000;
        private const uint SND_NODEFAULT = 0x0002;

        #region STRUCTURES
        // STRUCTURE CARTE
        public struct S_CarteDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE AMITIE
        public struct S_AmitieDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE AUCUNE TABLE
        public struct S_AucuneTableDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DECISION
        public struct S_DecisionDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE FELICITATION
        public struct S_FelicitationDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE INFO JOUEUR
        public struct S_InfoJoueurDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE FIREFOX GGS VISIBLE
        public struct S_JeNeSuisPasAssisDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE JOUEUR A TAPIS
        public struct S_JoueurATapisDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE JOUEUR PAROLE
        public struct S_JoueurParoleDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE JOUEUR REFLECHIT
        public struct S_JoueurReflechitDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE JOUEUR RELANCE
        public struct S_JoueurRelanceDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE JOUEUR SE COUCHE
        public struct S_JoueurSeCoucheDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE JOUEUR SUIT
        public struct S_JoueurSuitDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE GAGNE
        public struct S_GagneDetect
        {
            public int Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE MAIN BOARD
        public struct S_MainBoardDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE NIVEAU TABLE
        public struct S_NiveauTableDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE NOUVEAU NIVEAU
        public struct S_NouveauNiveauDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE PLACES LIBRES
        public struct S_PlacesLibresDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE PLACES OCCUPEE
        public struct S_PlaceOccupeeDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE PLUS DE JETONS
        public struct S_PlusDeJetonsDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE PLUS DE POT
        public struct S_PlusDePotDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE POSITION BOUTON
        public struct S_PositionBoutonDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE PUBLICITE
        public struct S_PubliciteDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE PUPITRE GRIS
        public struct S_PupitreGrisDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE SUIS JE ASSIS
        public struct S_SuisJeAssisDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        public struct S_TablePreteDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }
        public struct S_TableStabiliseeDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }
        // STRUCTURE PREFERENCES UTILISATEUR
        public struct S_PreferencesUtilisateurDetect
        {
            public bool ModeDebug;
            public int DelaiPriseControle;
            public bool SeRecave;
            public int NiveauMise;
            public int Memo;
        }
        #endregion

        #region DETECTIONS
        // 572 pixels injectés 
        // TABLEAU DETECTION CARTES
        private S_CarteDetect[] Main1Detect = new S_CarteDetect[52];
        private S_CarteDetect[] Main2Detect = new S_CarteDetect[52];

        private S_CarteDetect[] Flop1Detect = new S_CarteDetect[52];
        private S_CarteDetect[] Flop2Detect = new S_CarteDetect[52];
        private S_CarteDetect[] Flop3Detect = new S_CarteDetect[52];

        private S_CarteDetect[] TurnDetect = new S_CarteDetect[52];
        private S_CarteDetect[] Turn_HBDetect = new S_CarteDetect[104];

        private S_CarteDetect[] RiverDetect = new S_CarteDetect[52];
        private S_CarteDetect[] River_HBDetect = new S_CarteDetect[104];

        // 217 pixels injectés 
        // TABLEAU DETECTION AMITIE
        private S_AmitieDetect[] AmitieDetect = new S_AmitieDetect[1];

        // TABLEAU DETECTION AUCUNE TABLE
        private S_AucuneTableDetect[] AucuneTableDetect = new S_AucuneTableDetect[1];

        // TABLEAU DETECTION DECISION
        private S_DecisionDetect[] DecisionDetect = new S_DecisionDetect[5];

        // TABLEAU DETECTION FELICITATION
        private S_FelicitationDetect[] FelicitationDetect = new S_FelicitationDetect[2];

        // TABLEAU DETECTION INFO JOUEUR
        private S_InfoJoueurDetect[] InfoJoueurDetect = new S_InfoJoueurDetect[1];

        // TABLEAU DETECTION J AI QUITTE MA PLACE VISIBLE
        private S_JeNeSuisPasAssisDetect[] JeNeSuisPasAssisDetect = new S_JeNeSuisPasAssisDetect[1];

        // TABLEAU DETECTION JOUEUR A TAPIS
        private S_JoueurATapisDetect[] JoueurATapisDetect = new S_JoueurATapisDetect[9];

        // TABLEAU DETECTION JOUEUR PAROLE
        private S_JoueurParoleDetect[] JoueurParoleDetect = new S_JoueurParoleDetect[9];

        // TABLEAU DETECTION JOUEUR REFLECHIT
        private S_JoueurReflechitDetect[] JoueurReflechitDetect = new S_JoueurReflechitDetect[9];

        // TABLEAU DETECTION JOUEUR RELANCE
        private S_JoueurRelanceDetect[] JoueurRelanceDetect = new S_JoueurRelanceDetect[9];

        // TABLEAU DETECTION JOUEUR SE COUCHE
        private S_JoueurSeCoucheDetect[] JoueurSeCoucheDetect = new S_JoueurSeCoucheDetect[9];

        // TABLEAU DETECTION JOUEUR SUIT
        private S_JoueurSuitDetect[] JoueurSuitDetect = new S_JoueurSuitDetect[9];

        // TABLEAU DETECTION GAGNE
        private S_GagneDetect[] GagneDetect = new S_GagneDetect[18];

        // TABLEAU DETECTION EVENEMENTIELLE
        private S_MainBoardDetect[] MainBoardDetect = new S_MainBoardDetect[82];

        // TABLEAU DETECTION NIVEAU TABLE
        private S_NiveauTableDetect[] NiveauTableDetect = new S_NiveauTableDetect[12];

        // TABLEAU DETECTION NOUVEAU NIVEAU
        private S_NouveauNiveauDetect[] NouveauNiveauDetect = new S_NouveauNiveauDetect[1];

        // TABLEAU DETECTION PLACE LIBRE
        private S_PlacesLibresDetect[] PlacesLibresDetect = new S_PlacesLibresDetect[18];

        // TABLEAU DETECTION PLACE RESERVE
        private S_PlaceOccupeeDetect[] PlaceOccupeeDetect = new S_PlaceOccupeeDetect[1];

        // TABLEAU DETECTION PLUS DE JETONS
        private S_PlusDeJetonsDetect[] PlusDeJetonsDetect = new S_PlusDeJetonsDetect[1];

        // TABLEAU DETECTION PLUS DE POT
        private S_PlusDePotDetect[] PlusDePotDetect = new S_PlusDePotDetect[1];

        // TABLEAU DETECTION POSITION BOUTON
        private S_PositionBoutonDetect[] PositionBoutonDetect = new S_PositionBoutonDetect[9];

        // TABLEAU DETECTION PUBLCITE
        private S_PubliciteDetect[] PubliciteDetect = new S_PubliciteDetect[1];

        // TABLEAU DETECTION PUPITRE GRIS
        private S_PupitreGrisDetect[] PupitreGrisDetect = new S_PupitreGrisDetect[2];

        // TABLEAU DETECTION SUIS JE ASSIS
        private S_SuisJeAssisDetect[] SuisJeAssisDetect = new S_SuisJeAssisDetect[18];

        // TABLEAU DETECTION TABLE PRETE
        private S_TablePreteDetect[] TablePreteDetect = new S_TablePreteDetect[1];

        // TABLEAU DETECTION TABLE STABILISEE
        private S_TableStabiliseeDetect[] TableStabiliseeDetect = new S_TableStabiliseeDetect[2];

        // TABLEAU DETECTION PREFERENCES UTILISATEUR
        private S_PreferencesUtilisateurDetect PreferencesUtilisateur;

        // TOTAL : 789 pixels injectés 

        #endregion

        private bool bQuitterRevenir = false;


        // =========================================================
        // ETAT DU MOTEUR BOARD
        // =========================================================
        private bool bMain1_Lu;
        private bool bMain2_Lu;

        private bool bFlop1_Lu;
        private bool bFlop2_Lu;
        private bool bFlop3_Lu;

        private bool bTurn_Lu;
        private bool bRiver_Lu;

        private bool bGris;

        private bool bFlop1_Present;
        private bool bFlop2_Present;
        private bool bFlop3_Present;
        private bool bAttente_Stabilisation_Apres_Assise = false;


        // =========================================================
        // RECAVE
        // =========================================================
        private bool bAttente_Recave = false;
        private DateTime Heure_Recave;

        // =========================================================
        // DECISION
        // =========================================================

        // -------------------------------------
        // Action choisie par l'IA
        // -------------------------------------
        private string ActionForcee = null;

        // -------------------------------------
        // Délais de décision
        // -------------------------------------
        private bool bAttente_Decision = false;
        private DateTime Heure_Decision;
        private int Delai_Decision = 0;

        // -------------------------------------
        // Attente de disparition du bouton
        // après le clic (anti double-clic)
        // -------------------------------------
        private bool bAttente_Disparition = false;
        private string Nom_Bouton_Attente = "";

        // -------------------------------------
        // Prise de contrôle manuelle
        // -------------------------------------
        private bool bModeManuel = false;

        private bool bModeDecision = false;

        // Délai entre l'appui et le relâchement du clic (ms)
        //private int DelaiClic = 10;

        private bool bChangerDePlace;

        private bool bSeRecaver;

        DateTime debutPub = DateTime.MinValue;

        private int Nb_Mains_Testees = 0;


        private bool bModeTest = false;

        // -------------------------------------
        // SON CALCULS
        // -------------------------------------
        private const int DELAI_TEST_LECTURE = 2000;

        // =========================================================
        // CARTES
        // =========================================================
        private string Carte;
        private string Main1;
        private string Main2;
        private string Flop1;
        private string Flop2;
        private string Flop3;
        private string Turn;
        private string River;
        private string M;
        private string M1_Val;
        private string M2_Val;
        private string Memo_Max_Suit;



        // =========================================================
        // VARIABLES
        // =========================================================
        private float flottant;
        private int entier;

        // =========================================================
        // SESSION
        // =========================================================
        private DateTime DebutSession;
        private DispatcherTimer timer;
        private DateTime DebutPause;

        private bool JeuEnPause = false;

        // =========================================================
        // STATISTIQUES
        // =========================================================
        private int NbMains = 0;
        private int NbTapis = 0;
        private int NbParole = 0;

        private bool bTir_Flop_Quinte, bTir_Flop_Ventrale, bTir_Flop_Couleur, bTir_Flop_QFlush;
        private bool bTir_Turn_Quinte, bTir_Turn_Ventrale, bTir_Turn_Couleur, bTir_Turn_QFlush;
        private bool bPreFlop_Paire, bPreFlop_Suite, bPreFlop_Pareille;
        private bool bFlop_Paire, bFlop_2_Paires, bFlop_Brelan, bFlop_Quinte, bFlop_Couleur, bFlop_Full, bFlop_Carre, bFlop_QFlush;
        private bool bTurn_Paire, bTurn_2_Paires, bTurn_Brelan, bTurn_Quinte, bTurn_Couleur, bTurn_Full, bTurn_Carre, bTurn_QFlush;
        private bool bRiver_Paire, bRiver_2_Paires, bRiver_Brelan, bRiver_Quinte, bRiver_Couleur, bRiver_Full, bRiver_Carre, bRiver_QFlush;
        private bool bDeuxPaires;

        private bool bPreFlop_Calcul;
        private bool bFlop_Calcul;
        private bool bFlop_Tirage;
        private bool bFlop_Danger;
        private bool bTurn_Calcul;
        private bool bTurn_Tirage;
        private bool bTurn_Danger;
        private bool bRiver_Calcul;
        private bool bRiver_Danger;

        //----------------------------------------------------------
        // MOTEUR
        //----------------------------------------------------------
        private bool bSuperviseurVert_Actif = false;
        private bool bSuperviseurActif = false;

        private bool bVoyantRougeAttente = false;
        private DateTime VoyantRouge = DateTime.MinValue;

        //----------------------------------------------------------
        // MODES
        //----------------------------------------------------------
        private bool bTestBoard_Actif = false;
        private bool bTriggersBoard_Actif = false;
        private bool bTapisPreFlop_Actif = false;
        private bool bIA_Actif = false;
        //----------------------------------------------------------

        //----------------------------------------------------------
        // Validation Flop
        //----------------------------------------------------------
        private string Flop_1 = "";
        private string Flop_2 = "";
        private string Flop_3 = "";
        //----------------------------------------------------------


        private bool bPoints = true;

        private System.Drawing.Bitmap Bitmap_Main;

        // =========================================================
        // AUTORISATION D'ECRITURE DANS L'HISTORIQUE
        // =========================================================
        private bool bAutoriserHistorique = true;

        private const string DOSSIER_JSON = @"F:\source\repos\Poker\JSON";
        private static readonly string FichierHistoriqueJSON = Path.Combine(DOSSIER_JSON, "Historique.json");


        HashSet<string> RangeTapis = new HashSet<string>();
        HashSet<string> RangeRelance = new HashSet<string>();
        HashSet<string> RangeSuivre = new HashSet<string>();
        HashSet<string> RangeCall = new HashSet<string>();
        HashSet<string> RangeFold = new HashSet<string>();
        Dictionary<string, Button> BoutonsRange;
        List<Button> BoutonsAutoMise = new List<Button>();
        List<Button> BoutonsMemo = new List<Button>();
        List<Button> BoutonsNiveauTable = new List<Button>();
        List<Button> BoutonsMise = new List<Button>();
        List<Button> BoutonsPosition = new List<Button>();
        List<Button> BoutonsPlaceDisponible = new List<Button>();
        List<Button> BoutonsBoutonPosition = new List<Button>();
        List<Button> BoutonsJoueur_Mise = new List<Button>();
        List<Button> BoutonsBouton_en = new List<Button>();

        int niveauMemoAuto = 0;

        private int niveauMise = 0;
        private int MiseSelectionnee;
        int NbTapisGagne = 0;
        bool autoActif = false;
        List<Button> NiveauxMise = new List<Button>();
        bool premierGO = true;
        bool niveauInitialise = false;
        DispatcherTimer timerGoFlash;
        bool flashGO = false;
        int compteurFlashGO = 0;

        private bool bDebug = false;

        private bool bArretUrgence = false;

        // Carte sélectionnée manuellement (clic sur la grille 52 cartes)
        private string CarteCourante = "";

        // Indique si le paquet a été initialisé
        private bool PaquetInitialise = false;

        private readonly string[] Valeurs = { "A", "K", "Q", "J", "10", "9", "8", "7", "6", "5", "4", "3", "2" };

        private readonly string[] Symboles = { "♣", "♦", "♥", "♠" };

        // Générateur aléatoire
        Random rnd = new Random();

        // Paquet de cartes (0..51)
        List<int> Paquet = new List<int>();

        private Jeu JeuPreFlop = new Jeu();
        private Jeu JeuFlop = new Jeu();
        private Jeu JeuTurn = new Jeu();
        private Jeu JeuRiver = new Jeu();

        // =========================================================
        // 🔥 TABLE DES CARTES EN COURS
        // =========================================================
        private List<Carte> TableCartes = new List<Carte>();

        public MainWindow()
        {
            InitializeComponent();


            ImagesMonAvatar = new[]
            {
                Mon_Avatar_1,
                Mon_Avatar_2,
                Mon_Avatar_3,
                Mon_Avatar_4,
                Mon_Avatar_5,
                Mon_Avatar_6,
                Mon_Avatar_7,
                Mon_Avatar_8,
                Mon_Avatar_9
            };

            // Tous cachés au démarrage
            foreach (Image img in ImagesMonAvatar)
                img.Visibility = Visibility.Hidden;

            PreviewKeyDown += MainWindow_PreviewKeyDown;


            // =====================================
            // Chargement des préférences utilisateur
            // =====================================
            // Les préférences sont chargées par Superviseur_Poker()

            // =====================================
            // Application du mode Debug
            // =====================================
            if (bDebug)
                Mode_Debug_ON();
            else
                Mode_Debug_OFF();

            // =====================================
            // Chargement des JSON
            // =====================================
            LbNbTapis.Content = "0";

            Charger_Main1_JSON();
            Charger_Main2_JSON();

            Charger_Flop1_JSON();
            Charger_Flop2_JSON();
            Charger_Flop3_JSON();

            Charger_Turn_JSON();
            Charger_Turn_HB_JSON();
            Charger_River_JSON();
            Charger_River_HB_JSON();

            Charger_Amitie_JSON();
            Charger_Aucune_Table_JSON();
            Charger_Cartographie_Du_Pot_JSON();
            Charger_Decision_JSON();
            Charger_Felicitation_JSON();
            Charger_Info_Joueur_JSON();
            Charger_Je_Ne_Suis_Pas_Assis_JSON();

            Charger_Joueur_A_Tapis_JSON();
            Charger_Joueur_Parole_JSON();
            Charger_Joueur_Reflechit_JSON();
            Charger_Joueur_Relance_JSON();
            Charger_Joueur_Se_Couche_JSON();
            Charger_Joueur_Suit_JSON();

            Charger_Gagne_JSON();
            Charger_Main_Board_JSON();
            Charger_Niveau_Table_JSON();
            Charger_Places_Libres_JSON();
            Charger_Place_Occupee_JSON();
            Charger_Position_Bouton_JSON();
            Charger_Plus_De_Jetons_JSON();
            Charger_Plus_De_Pot_JSON();
            Charger_Publicite_JSON();
            Charger_Pupitre_Gris_JSON();

            Charger_Range_Tapis_JSON();
            Charger_Range_Relance_JSON();
            Charger_Range_Suivre_JSON();

            Charger_Suis_Je_Assis_JSON();
            Charger_Table_Prete_JSON();
            Charger_Table_Stabilisee_JSON();

            // =====================================
            // Variables
            // =====================================
            DebutSession = DateTime.Now;

            // =====================================
            // Timer principal
            // =====================================
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            // =====================================
            // Gestion pause automatique
            // =====================================
            this.StateChanged += Window_StateChanged;

            // =====================================
            // Combo Random
            // =====================================
            for (int i = 0; i <= 12; i++)
            {
                CbRandom.Items.Add(i);
            }

            // =====================================
            // Démarrage lorsque la fenêtre est prête
            // =====================================
            this.Loaded += MainWindow_Loaded;


            CbRandom.SelectedIndex = 0;
            MettreAJourIndicateurRandom();

            Orange = (Brush)FindResource("orange");
            Blanc = (Brush)FindResource("blanc");
            Bleu = (Brush)FindResource("bleu");
            Vert = (Brush)FindResource("vert");
            Rouge = (Brush)FindResource("rouge");
            Gold = (Brush)FindResource("gold");

            // =====================================
            // MODE AUTO AU DEMARRAGE
            // =====================================
            bModeManuel = false;

            Bt_Manuel.Background = Vert;
            Bt_Manuel.Content = "Auto";

            NiveauxMise = new List<Button>
            {
                Bt_M9,
                Bt_M8,
                Bt_M6,
                Bt_M4,
                Bt_M2,
                Bt_0,
                Bt_P2,
                Bt_P4,
                Bt_P6,
                Bt_P8,
                Bt_P10
            };

            BoutonsMemo = new List<Button>
            {
                Bt_Mem01,
                Bt_Mem02,
                Bt_Mem03,
                Bt_Mem04,
                Bt_Mem05,
                Bt_Mem06,
                Bt_Mem07,
                Bt_Mem08,
                Bt_Mem09,
                Bt_Mem10,
                Bt_Mem11,
                Bt_Mem12,
                Bt_Mem13
            };

            BoutonsNiveauTable = new List<Button>
            {
                Bt_Greenhorn,
                Bt_Beginner,
                Bt_Gambler,
                Bt_Pro,
                Bt_BigFish,
                Bt_Master,
                Bt_Champion,
                Bt_Guru,
                Bt_Ultimate,
                Bt_Epic,
                Bt_Insane,
                Bt_Legendary
            };

            BoutonsMise = new List<Button>
            {
                Bt_M9,
                Bt_M8,
                Bt_M6,
                Bt_M4,
                Bt_M2,
                Bt_0,
                Bt_P2,
                Bt_P4,
                Bt_P6,
                Bt_P8,
                Bt_P10
            };

            BoutonsPosition = new List<Button>
            {
                Bt_Position_1,
                Bt_Position_2,
                Bt_Position_3,
                Bt_Position_4,
                Bt_Position_5,
                Bt_Position_6,
                Bt_Position_7,
                Bt_Position_8,
                Bt_Position_9
            };

            BoutonsPlaceDisponible = new List<Button>
            {
                Bt_PlaceDisponible_1,
                Bt_PlaceDisponible_2,
                Bt_PlaceDisponible_3,
                Bt_PlaceDisponible_4,
                Bt_PlaceDisponible_5,
                Bt_PlaceDisponible_6,
                Bt_PlaceDisponible_7,
                Bt_PlaceDisponible_8,
                Bt_PlaceDisponible_9
            };

            BoutonsBoutonPosition = new List<Button>
            {
                Bt_Bouton_en_1,
                Bt_Bouton_en_2,
                Bt_Bouton_en_3,
                Bt_Bouton_en_4,
                Bt_Bouton_en_5,
                Bt_Bouton_en_6,
                Bt_Bouton_en_7,
                Bt_Bouton_en_8,
                Bt_Bouton_en_9
            };

            BoutonsJoueur_Mise = new List<Button>
            {
                Bt_Joueur1_Mise,
                Bt_Joueur2_Mise,
                Bt_Joueur3_Mise,
                Bt_Joueur4_Mise,
                Bt_Joueur5_Mise,
                Bt_Joueur6_Mise,
                Bt_Joueur7_Mise,
                Bt_Joueur8_Mise,
                Bt_Joueur9_Mise
            };

            BoutonsRange = new Dictionary<string, Button>()
            {
                // ligne1 AA...22
                { "AA",   Bt_AA },
                { "KK",   Bt_KK },
                { "QQ",   Bt_QQ },
                { "JJ",   Bt_JJ },
                { "1010", Bt_1010 },
                { "99",   Bt_99 },
                { "88",   Bt_88 },
                { "77",   Bt_77 },
                { "66",   Bt_66 },
                { "55",   Bt_55 },
                { "44",   Bt_44 },
                { "33",   Bt_33 },
                { "22",   Bt_22 }, 
                // ligne2 AKp...A2p
                { "AKp",  Bt_AKp },
                { "AQp",  Bt_AQp },
                { "AJp",  Bt_AJp },
                { "A10p", Bt_A10p },
                { "A9p",  Bt_A9p },
                { "A8p",  Bt_A8p },
                { "A7p",  Bt_A7p },
                { "A6p",  Bt_A6p },
                { "A5p",  Bt_A5p },
                { "A4p",  Bt_A4p },
                { "A3p",  Bt_A3p },
                { "A2p",  Bt_A2p },
                // ligne3 AK...A2
                { "AK",   Bt_AK },
                { "AQ",   Bt_AQ },
                { "AJ",   Bt_AJ },
                { "A10",  Bt_A10 },
                { "A9",   Bt_A9 },
                { "A8",   Bt_A8 },
                { "A7",   Bt_A7 },
                { "A6",   Bt_A6 },
                { "A5",   Bt_A5 },
                { "A4",   Bt_A4 },
                { "A3",   Bt_A3 },
                { "A2",   Bt_A2 },
                // ligne4 KQp...K2p
                { "KQp",  Bt_KQp },
                { "KJp",  Bt_KJp },
                { "K10p", Bt_K10p },
                { "K9p",  Bt_K9p },
                { "K8p",  Bt_K8p },
                { "K7p",  Bt_K7p },
                { "K6p",  Bt_K6p },
                { "K5p",  Bt_K5p },
                { "K4p",  Bt_K4p },
                { "K3p",  Bt_K3p },
                { "K2p",  Bt_K2p },
                // ligne4 KQ K10 K9 K8 K7 K6 QJp Q10p Q9p J10p
                { "KQ",   Bt_KQ },
                { "KJ",   Bt_KJ },
                { "K10",  Bt_K10 },
                { "K9",   Bt_K9 },
                { "K8",   Bt_K8 },
                { "K7",   Bt_K7 },
                { "K6",   Bt_K6 },
                { "QJp",   Bt_QJp },
                { "Q10p",  Bt_Q10p },
                { "Q9p",   Bt_Q9p },
                { "J10p",  Bt_J10p },
            };

            BoutonsAutoMise = new List<Button>
            {
                Bt_Auto_M9,
                Bt_Auto_M8,
                Bt_Auto_M6,
                Bt_Auto_M4,
                Bt_Auto_M2,
                Bt_Auto_0,
                Bt_Auto_P2,
                Bt_Auto_P4,
                Bt_Auto_P6,
                Bt_Auto_P8
            };


            if (Bt_Se_Recave.Background.Equals((Brush)FindResource("blanc")))
            {
                Bt_Se_Recave_Click(null, null);
            }

            ActiverMise(Bt_0);

            PaquetInitialise = false;

            CreerZonesCartes();

            // 🔥 Activation auto des captures
            Brush blanc = (Brush)FindResource("blanc");

            if (Bt_Joueur1_Mise.Background == blanc)
                Bt_Joueur_Mise_Click(Bt_Joueur1_Mise, null);

            if (Bt_Joueur2_Mise.Background == blanc)
                Bt_Joueur_Mise_Click(Bt_Joueur2_Mise, null);


        }
        // =========================================================
        // 🔥 NORMALISER MAIN TAPIS PREFLOP
        // =========================================================
        private string NormaliserMainTapis(
            CarteInfo carte1,
            CarteInfo carte2)
        {
            string[] valeurs =
            {
                "2", "3", "4", "5", "6", "7",
                "8", "9", "10", "J", "Q", "K", "A"
            };

            // =====================================
            // SECURITE
            // =====================================
            if (carte1.V < 1 || carte1.V > 13)
                return null;

            if (carte2.V < 1 || carte2.V > 13)
                return null;

            string v1 = valeurs[carte1.V - 1];
            string v2 = valeurs[carte2.V - 1];

            // =====================================
            // PAIRE
            // =====================================
            if (carte1.V == carte2.V)
                return v1 + v2;

            // =====================================
            // PLUS FORTE EN PREMIER
            // =====================================
            if (carte2.V > carte1.V)
            {
                string temp = v1;
                v1 = v2;
                v2 = temp;
            }

            // =====================================
            // SUITED
            // =====================================
            bool suited =
                carte1.C == carte2.C;

            return suited
                ? v1 + v2 + "p"
                : v1 + v2;
        }
        #region DIVERS

        private void ToggleRangeTapis(string main)
        {
            if (EstDansRange(main))
                RangeTapis.Remove(main);
            else
                RangeTapis.Add(main);
        }
        private void AppliquerRandomSiActif()
        {
            if (CbRandom.SelectedItem == null)
                return;

            int niveau = (int)CbRandom.SelectedItem;

            // =====================================
            // RANDOM OFF
            // =====================================
            if (niveau == 0)
                return;

            // =====================================
            // TIRAGE ENTRE MEMO 1 ET MEMO N
            // niveau = 1 → index 0
            // niveau = 5 → index 0..4
            // niveau = 12 → index 0..11
            // niveau = 13 → index 0..12
            // =====================================
            int indexRandom = rnd.Next(niveau);

            // =====================================
            // ACTIVER LE MEMO TIRE
            // =====================================
            BoutonsMemo[indexRandom].RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
        }

        // Carte tirée (tirage complet de la main)
        struct CarteTirage
        {
            public string Carte;   // "A♠"
            public bool Affichee;  // true / false
        }

        // Tirage des 7 cartes max (2 main + 5 board)
        CarteTirage[] Tirage = new CarteTirage[7];

        // Carte utilisée par le moteur d'analyse
        struct CarteInfo
        {
            public int Numero, N;   // 0..51
            public int Couleur, C;  // 0..3
            public int Valeur, V;   // 1..13 (2..As)
        }

        // Table moteur des 7 cartes
        CarteInfo[] T_Carte = new CarteInfo[7];

        // Crée une grille invisible 4x13 au-dessus de l'image des cartes
        // Chaque zone représente une carte et renvoie sa valeur via Tag
        private void CreerZonesCartes()
        {
            double largeur = 128 / 4.0;
            double hauteur = 260 / 13.0;

            for (int ligne = 0; ligne < 13; ligne++)
            {
                for (int col = 0; col < 4; col++)
                {
                    Border zone = new Border
                    {
                        Width = largeur,
                        Height = hauteur,
                        Background = Brushes.Transparent,
                        Tag = $"{Valeurs[ligne]}{Symboles[col]}"
                    };

                    Canvas.SetLeft(zone, col * largeur);
                    Canvas.SetTop(zone, ligne * hauteur);

                    zone.MouseLeftButtonDown += Carte_Click;
                    ZoneCartes.Children.Add(zone);
                }
            }
        }
        public int Numero, N;
        public int Couleur, C;
        public int Valeur, V;

        // Convertit "10♠" ou "A♥" en données moteur exploitables
        private CarteInfo ExtraireCarte(string carte)
        {
            string[] valeurs =
            {
                "2", "3", "4", "5", "6", "7",
                "8", "9", "10", "J", "Q", "K", "A"
            };

            string couleurs = "♣♦♥♠";

            // =====================================
            // SÉPARER VALEUR / COULEUR
            // =====================================
            string val =
                carte.Substring(0, carte.Length - 1);

            char coul =
                carte[carte.Length - 1];

            int iv =
                Array.IndexOf(valeurs, val);

            int ic =
                couleurs.IndexOf(coul);

            int valeur = iv + 1;
            int couleur = ic;
            int numero = iv * 4 + ic;

            return new CarteInfo
            {
                // Champs principaux
                Valeur = valeur,
                Couleur = couleur,
                Numero = numero,

                // Alias utilisés par le moteur
                V = valeur,
                C = couleur,
                N = numero
            };
        }

        // Affiche une carte dans une TextBox
        // Gère : affichage, couleur, moteur, état des boutons
        private string AffichageCarte(string carte)
        {
            if (carte.StartsWith("T"))
                return "10" + carte[1];

            return carte;
        }

        private void AfficherCarte(TextBox tb, Button precedent, Button courant, int indexCarte)
        {

            if (string.IsNullOrEmpty(CarteCourante))
            {
                return;
            }

            // contrôle ordre via couleur
            if (precedent != null && precedent.Background != Orange)
            {
                return;
            }

            // affichage
            tb.Text = AffichageCarte(CarteCourante);
            tb.Foreground = CouleurCarte(CarteCourante);

            // moteur
            T_Carte[indexCarte] = ExtraireCarte(CarteCourante);

            // couleurs boutons
            if (precedent != null)
                precedent.Background = Blanc;

            courant.Background = Orange;
        }

        private void AfficherCarte_Test(TextBox tb, Button bt, int indexCarte)
        {
            if (string.IsNullOrEmpty(CarteCourante))
                return;

            // =====================================
            // AFFICHAGE
            // =====================================

            tb.Text = AffichageCarte(CarteCourante);

            tb.Foreground = CouleurCarte(CarteCourante);

            // =====================================
            // MOTEUR
            // =====================================

            T_Carte[indexCarte] = ExtraireCarte(CarteCourante);

            // =====================================
            // BOUTON
            // =====================================
            if (!bTestBoard_Actif)
                bt.Background = Orange;
        }

        private void InitialiserPaquet()
        {
            Paquet.Clear();
            for (int i = 0; i < 52; i++)
                Paquet.Add(i);
        }

        // 🔹 Sélection carte
        private void Carte_Click(object sender, MouseButtonEventArgs e)
        {
            CarteCourante = (string)((Border)sender).Tag;
        }

        public Brush Orange { get; private set; }
        public Brush Blanc { get; private set; }
        public Brush Bleu { get; private set; }
        public Brush Vert { get; private set; }
        public Brush Gold { get; private set; }
        public Brush Rouge { get; private set; }

        private void Initialise_Boutons_Main_Flop_Turn_River()
        {
            Bt_Main_1.Background = Orange;
            Bt_Main_2.Background = Blanc;
            Bt_Flop_1.Background = Blanc;
            Bt_Flop_2.Background = Blanc;
            Bt_Flop_3.Background = Blanc;
            Bt_Turn.Background = Blanc;
            Bt_River.Background = Blanc;
            Bt_Rand_M.Background = Blanc;
            Bt_Rand_F.Background = Blanc;
            Bt_Rand_T.Background = Blanc;
            Bt_Rand_R.Background = Blanc;
        }

        private void Efface_Affichage_Cartes()
        {
            TB_M1.Clear();
            TB_M2.Clear();
            TB_F1.Clear();
            TB_F2.Clear();
            TB_F3.Clear();
            TB_T.Clear();
            TB_R.Clear();
        }

        private void MettreAJourIndicateurRandom()
        {
            int niveau = (int)CbRandom.SelectedItem;

            if (niveau == 0)
            {
                LbRandomStatus.Content = "● RANDOM OFF";
                LbRandomStatus.Background =
                    new SolidColorBrush(Color.FromRgb(230, 230, 230));
                LbRandomStatus.Foreground = Brushes.Black;
                return;
            }

            LbRandomStatus.Content = "● RANDOM ON";

            if (niveau <= 4)
            {
                LbRandomStatus.Background =
                    new SolidColorBrush(Color.FromRgb(120, 200, 120));
                LbRandomStatus.Foreground = Brushes.Black;
            }
            else if (niveau <= 8)
            {
                LbRandomStatus.Background =
                    new SolidColorBrush(Color.FromRgb(255, 170, 60));
                LbRandomStatus.Foreground = Brushes.Black;
            }
            else
            {
                LbRandomStatus.Background =
                    new SolidColorBrush(Color.FromRgb(210, 70, 70));
                LbRandomStatus.Foreground = Brushes.White;
            }
        }
        // =========================================================
        // 🔥 NOM DE LA CARTE
        // =========================================================
        private string NomCarte(int valeur)
        {
            switch (valeur)
            {
                case 14: return "As";
                case 13: return "Roi";
                case 12: return "Dame";
                case 11: return "Valet";
                default: return valeur.ToString();
            }
        }

        // =========================================================
        // 🔥 MAIN 1
        // =========================================================
        private void Bt_Main_1_Click(object sender, RoutedEventArgs e)
        {
            // =====================================
            // 🔥 RESET TAPIS PREFLOP
            // Nouvelle main
            // =====================================
            Bt_Tapis.Background = Blanc;
            // =====================================
            // ARRÊTER LE CLIGNOTEMENT DANGER
            // Nouvelle main
            // =====================================
            StopFlashDanger();

            // =====================================
            // NOUVELLE PARTIE TEST
            // =====================================
            TirageFlop.Quinte = false;
            TirageFlop.Ventrale = false;
            TirageFlop.Couleur = false;
            TirageFlop.QuinteFlush = false;
            TirageFlop.Participation = 0;
            TirageFlop.Max = false;
            TirageFlop.CarteHaute = 0;

            TirageFlop.DangerCarre = false;
            TirageFlop.DangerFull = false;
            TirageFlop.DangerQuinte = false;
            TirageFlop.DangerCouleur = false;
            TirageFlop.DangerQuinteFlush = false;

            TirageTurn.Quinte = false;
            TirageTurn.Ventrale = false;
            TirageTurn.Couleur = false;
            TirageTurn.QuinteFlush = false;
            TirageTurn.Participation = 0;
            TirageTurn.Max = false;
            TirageTurn.CarteHaute = 0;

            TirageTurn.DangerCarre = false;
            TirageTurn.DangerFull = false;
            TirageTurn.DangerQuinte = false;
            TirageTurn.DangerCouleur = false;
            TirageTurn.DangerQuinteFlush = false;

            // =====================================
            // RESET AFFICHAGE TIRAGES
            // =====================================
            Bt_Tir_Flop_Quinte.Background = Blanc;
            Bt_Tir_Flop_Quinte.Content = "Q";

            Bt_Tir_Flop_Couleur.Background = Blanc;
            Bt_Tir_Flop_Quinte_Flush.Background = Blanc;

            Bt_Tir_Turn_Quinte.Background = Blanc;
            Bt_Tir_Turn_Quinte.Content = "Q";

            Bt_Tir_Turn_Couleur.Background = Blanc;
            Bt_Tir_Turn_Quinte_Flush.Background = Blanc;

            // =====================================
            // RESET BOUTON DANGER
            // =====================================
            Bt_Danger.Background = Blanc;
            Bt_Danger.Foreground = Brushes.Black;
            Bt_Danger.Content = "";

            // =====================================
            // 🔥 RESET DÉCISION PREFLOP — VERSION TEST
            // =====================================
            Bt_Tapis.Background = Blanc;
            Bt_Parole_Passe.Background = Blanc;

            // =====================================
            // RESET FLAGS SONS
            // =====================================
            bPreFlop_Calcul = false;

            bFlop_Calcul = false;
            bFlop_Tirage = false;
            bFlop_Danger = false;

            bTurn_Calcul = false;
            bTurn_Tirage = false;
            bTurn_Danger = false;

            bRiver_Calcul = false;
            bRiver_Danger = false;

            // =====================================
            // LECTURE MAIN 1
            // =====================================
            AfficherCarte(
                TB_M1,
                null,
                Bt_Main_1,
                0);
        }


        // =========================================================
        // 🔥 MAIN 2
        // =========================================================
        private void Bt_Main_2_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte(
                TB_M2,
                Bt_Main_1,
                Bt_Main_2,
                1);

            // =====================================
            // CALCUL PRE-FLOP
            // =====================================
            Calcul_PreFlop();

            // =====================================
            // MODE TAPIS PREFLOP ACTIF ?
            // =====================================
            if (bTapisPreFlop_Actif)
            {
                Tapis_PreFlop_Decision();
            }
        }


        private void Bt_Flop_1_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte(
                TB_F1,
                Bt_Main_2,
                Bt_Flop_1,
                2);
        }


        private void Bt_Flop_2_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte(
                TB_F2,
                Bt_Flop_1,
                Bt_Flop_2,
                3);
        }


        // =========================================================
        // 🔥 FLOP TEST
        // =========================================================
        private void Bt_Flop_3_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte(
                TB_F3,
                Bt_Flop_2,
                Bt_Flop_3,
                4);

            // =====================================
            // CALCUL COMBINAISON FLOP
            // =====================================
            bFlop_Calcul = true;

            Calcul_Flop();

            // =====================================
            // 🔥 SON COMBINAISON FLOP
            // =====================================
            if (bFlop_QFlush)
                JouerSon("Quinte Flush.wav");
            else if (bFlop_Carre)
                JouerSon("Carre.wav");
            else if (bFlop_Full)
                JouerSon("Full.wav");
            else if (bFlop_Couleur)
                JouerSon("Couleur.wav");
            else if (bFlop_Quinte)
                JouerSon("Quinte.wav");
            else if (bFlop_Brelan)
                JouerSon("Brelan.wav");
            else if (bFlop_2_Paires)
                JouerSon("Deux Paires.wav");
            else if (bFlop_Paire)
                JouerSon("Paire.wav");

            bFlop_Calcul = false;

            // =====================================
            // CALCUL TIRAGES + DANGERS
            // =====================================
            Calcul_Tirages();

            // =====================================
            // SON TIRAGE FLOP
            // =====================================
            if (TirageFlop.QuinteFlush)
            {
                JouerSon("Tirage Quinte Flush.wav");
            }
            else if (TirageFlop.Couleur)
            {
                JouerSon("Tirage Couleur.wav");
            }
            else if (TirageFlop.Quinte)
            {
                if (TirageFlop.Ventrale)
                    JouerSon("Ventrale.wav");
                else
                    JouerSon("Tirage Quinte.wav");
            }

            // =====================================
            // SON DANGER FLOP
            // =====================================
            bFlop_Danger = true;

            Afficher_Danger(TirageFlop);

            if (TirageFlop.DangerCarre)
            {
                JouerSon("Danger Carre.wav");
            }
            else if (TirageFlop.DangerFull)
            {
                JouerSon("Danger Full.wav");
            }
            else if (TirageFlop.DangerQuinteFlush)
            {
                JouerSon("Danger Quinte Flush.wav");
            }
            else if (TirageFlop.DangerCouleur &&
                     TirageFlop.DangerQuinte)
            {
                JouerSon("Danger Couleur Quinte.wav");
            }
            else if (TirageFlop.DangerCouleur)
            {
                JouerSon("Danger Couleur.wav");
            }
            else if (TirageFlop.DangerQuinte)
            {
                JouerSon("Danger Quinte.wav");
            }

            bFlop_Danger = false;
        }

        // =========================================================
        // 🔥 TURN TEST
        // =========================================================
        private void Bt_Turn_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte_Test(TB_T, Bt_Turn, 5);

            // =====================================
            // CALCUL COMBINAISON TURN
            // =====================================
            Calcul_Turn();

            // =====================================
            // SON COMBINAISON TURN
            // =====================================
            if (bTurn_QFlush)
                JouerSon("Quinte Flush.wav");
            else if (bTurn_Carre)
                JouerSon("Carre.wav");
            else if (bTurn_Full)
                JouerSon("Full.wav");
            else if (bTurn_Couleur)
                JouerSon("Couleur.wav");
            else if (bTurn_Quinte)
                JouerSon("Quinte.wav");
            else if (bTurn_Brelan)
                JouerSon("Brelan.wav");
            else if (bTurn_2_Paires)
                JouerSon("Deux Paires.wav");
            else if (bTurn_Paire)
                JouerSon("Paire.wav");

            // =====================================
            // CALCUL TIRAGES + DANGERS TURN
            // =====================================
            Calcul_Tirages();

            // =====================================
            // SON TIRAGE TURN
            // =====================================
            if (TirageTurn.QuinteFlush)
            {
                JouerSon("Tirage Quinte Flush.wav");
            }
            else if (TirageTurn.Couleur)
            {
                JouerSon("Tirage Couleur.wav");
            }
            else if (TirageTurn.Quinte)
            {
                if (TirageTurn.Ventrale)
                    JouerSon("Ventrale.wav");
                else
                    JouerSon("Tirage Quinte.wav");
            }

            // =====================================
            // DANGER TURN
            // =====================================
            bTurn_Danger = true;

            // =====================================
            // AFFICHAGE DANGER
            // =====================================
            Afficher_Danger(TirageTurn);

            // =====================================
            // SON DANGER TURN
            // =====================================
            if (TirageTurn.DangerCarre)
            {
                JouerSon("Danger Carre.wav");
            }
            else if (TirageTurn.DangerFull)
            {
                JouerSon("Danger Full.wav");
            }
            else if (TirageTurn.DangerQuinteFlush)
            {
                JouerSon("Danger Quinte Flush.wav");
            }
            else if (TirageTurn.DangerCouleur &&
                     TirageTurn.DangerQuinte)
            {
                JouerSon("Danger Couleur Quinte.wav");
            }
            else if (TirageTurn.DangerCouleur)
            {
                JouerSon("Danger Couleur.wav");
            }
            else if (TirageTurn.DangerQuinte)
            {
                JouerSon("Danger Quinte.wav");
            }

            bTurn_Danger = false;
        }

        // =========================================================
        // 🔥 RIVER TEST
        // =========================================================
        private void Bt_River_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte_Test(
                TB_R,
                Bt_River,
                6);

            // =====================================
            // CALCUL COMBINAISON RIVER
            // =====================================
            Calcul_River();

            // =====================================
            // SON COMBINAISON RIVER
            // =====================================
            if (bRiver_QFlush)
                JouerSon("Quinte Flush.wav");
            else if (bRiver_Carre)
                JouerSon("Carre.wav");
            else if (bRiver_Full)
                JouerSon("Full.wav");
            else if (bRiver_Couleur)
                JouerSon("Couleur.wav");
            else if (bRiver_Quinte)
                JouerSon("Quinte.wav");
            else if (bRiver_Brelan)
                JouerSon("Brelan.wav");
            else if (bRiver_2_Paires)
                JouerSon("Deux Paires.wav");
            else if (bRiver_Paire)
                JouerSon("Paire.wav");

            // =====================================
            // CALCUL TIRAGES + DANGERS RIVER
            // =====================================
            Calcul_Tirages();

            // =====================================
            // DANGER RIVER
            // =====================================
            bRiver_Danger = true;

            // =====================================
            // AFFICHAGE DANGER
            // =====================================
            Afficher_Danger(TirageRiver);

            // =====================================
            // SON DANGER RIVER
            // =====================================
            if (TirageRiver.DangerCarre)
            {
                JouerSon("Danger Carre.wav");
            }
            else if (TirageRiver.DangerFull)
            {
                JouerSon("Danger Full.wav");
            }
            else if (TirageRiver.DangerQuinteFlush)
            {
                JouerSon("Danger Quinte Flush.wav");
            }
            else if (TirageRiver.DangerCouleur &&
                     TirageRiver.DangerQuinte)
            {
                JouerSon("Danger Couleur Quinte.wav");
            }
            else if (TirageRiver.DangerCouleur)
            {
                JouerSon("Danger Couleur.wav");
            }
            else if (TirageRiver.DangerQuinte)
            {
                JouerSon("Danger Quinte.wav");
            }

            bRiver_Danger = false;
        }

        private void InitialiserPartieTest()
        {
            Initialiser_bPreFlop();
            Initialiser_bFlop();
            Initialiser_bTurn();
            Initialiser_bRiver();

            // Remise à zéro des boutons
            // Remise à zéro des tirages
            // Remise à zéro des statistiques si nécessaire
        }

        private Brush CouleurCarte(string c)
        {
            if (c.Contains("♣")) return Brushes.Blue;
            if (c.Contains("♦")) return Brushes.Green;
            if (c.Contains("♥")) return Brushes.Red;
            if (c.Contains("♠")) return Brushes.Black;
            return Brushes.Black;
        }

        private string CarteDepuisNumero(int n)
        {
            string valeurs = "23456789TJQKA";
            string couleurs = "♣♦♥♠";

            int valeur = n / 4;
            int couleur = n % 4;

            return $"{valeurs[valeur]}{couleurs[couleur]}";
        }

        private string TirerCarte()
        {
            int index = rnd.Next(Paquet.Count);
            int carte = Paquet[index];
            Paquet.RemoveAt(index);

            return CarteDepuisNumero(carte);
        }

        private void AfficherDepuisTirage(int index, TextBox tb, Button bouton)
        {
            if (Tirage[index].Affichee)
                return;

            tb.Text = AffichageCarte(Tirage[index].Carte);
            tb.Foreground = CouleurCarte(Tirage[index].Carte);

            Tirage[index].Affichee = true;
            bouton.Background = Orange;
        }

        // Tire les 7 cartes dès le bouton M
        // Permet analyse PréFlop / Flop / Turn / River
        private void Bt_Rand_M_Click(object sender, RoutedEventArgs e)
        {
            Bt_J_ai_Gagne.Background = (Brush)FindResource("blanc");
            Bt_J_ai_Gagne.Background = (Brush)FindResource("blanc");
            InitialiserPaquet();

            // reset
            for (int i = 0; i < 7; i++)
                Tirage[i].Affichee = false;

            TB_F1.Clear();
            TB_F2.Clear();
            TB_F3.Clear();
            TB_T.Clear();
            TB_R.Clear();

            // 🔥 ICI (comme en manuel : après reset)
            AppliquerRandomSiActif();

            // tirage des 7 cartes
            for (int i = 0; i < 7; i++)
            {
                Tirage[i].Carte = TirerCarte();
                Tirage[i].Affichee = false;
                T_Carte[i] = ExtraireCarte(Tirage[i].Carte);
            }

            // affichage main
            AfficherDepuisTirage(0, TB_M1, Bt_Main_1);
            AfficherDepuisTirage(1, TB_M2, Bt_Main_2);

            // 🔥 évaluation après affichage
            EvaluerTapisDepuisTextBox();

            Bt_Main_1.Background = Blanc;
            Bt_Flop_2.Background = Blanc;
            Bt_Flop_3.Background = Blanc;
            Bt_Turn.Background = Blanc;
            Bt_River.Background = Blanc;
            Bt_Rand_R.Background = Blanc;
            Bt_Rand_M.Background = Orange;
            Bt_Rand_F.Background = Blanc;
            Bt_Rand_R.Background = Blanc;
            Bt_Rand_T.Background = Blanc;
            // 🔥 déclencher la logique comme en manuel
            Bt_Main_2_Click(null, null);
        }

        // =========================================================
        // 🔥 Révèle le FLOP
        // Autorise renvoi même après Turn/River
        // =========================================================
        private void Bt_Rand_F_Click(object sender, RoutedEventArgs e)
        {
            Bt_J_ai_Gagne.Background = (Brush)FindResource("blanc");

            // 🔒 Main obligatoire
            if (string.IsNullOrWhiteSpace(TB_M1.Text) ||
                string.IsNullOrWhiteSpace(TB_M2.Text))
                return;

            // 🔥 Réaffichage Flop
            AfficherDepuisTirage(2, TB_F1, Bt_Flop_1);
            AfficherDepuisTirage(3, TB_F2, Bt_Flop_2);
            AfficherDepuisTirage(4, TB_F3, Bt_Flop_3);

            // 🔥 Recalcul FLOP
            //Calcul_Flop();
            //Calcul_Tirages_Flop();

            // 🔥 Reset visuel
            Bt_Main_2.Background = Blanc;

            Bt_Flop_1.Background = Blanc;
            Bt_Flop_2.Background = Blanc;

            Bt_Turn.Background = Blanc;
            Bt_River.Background = Blanc;

            Bt_Rand_M.Background = Blanc;
            Bt_Rand_T.Background = Blanc;
            Bt_Rand_R.Background = Blanc;

            // 🔥 Etat actif
            Bt_Rand_F.Background = Orange;
        }

        // =========================================================
        // 🔥 Révèle le TURN
        // Autorise renvoi après River
        // =========================================================
        private void Bt_Rand_T_Click(object sender, RoutedEventArgs e)
        {
            Bt_J_ai_Gagne.Background = (Brush)FindResource("blanc");

            // 🔒 Flop obligatoire
            if (string.IsNullOrWhiteSpace(TB_F1.Text) ||
                string.IsNullOrWhiteSpace(TB_F2.Text) ||
                string.IsNullOrWhiteSpace(TB_F3.Text))
                return;

            // 🔥 Réaffichage Turn
            AfficherDepuisTirage(5, TB_T, Bt_Turn);

            // 🔥 Recalcul TURN
            //Calcul_Turn();
            //Calcul_Tirages_Turn();

            // 🔥 Reset visuel
            Bt_Flop_3.Background = Blanc;
            Bt_River.Background = Blanc;

            Bt_Rand_F.Background = Blanc;
            Bt_Rand_R.Background = Blanc;

            // 🔥 Etat actif
            Bt_Rand_T.Background = Orange;
        }

        // =========================================================
        // 🔥 Révèle la RIVER
        // Toujours rejouable
        // =========================================================
        private void Bt_Rand_R_Click(object sender, RoutedEventArgs e)
        {
            Bt_J_ai_Gagne.Background = (Brush)FindResource("blanc");
            // 🔒 Turn obligatoire
            if (string.IsNullOrWhiteSpace(TB_T.Text))
                return;
            // 🔥 Réaffichage River
            AfficherDepuisTirage(6, TB_R, Bt_River);
            // 🔥 Recalcul RIVER
            //Calcul_River();
            // 🔥 Reset visuel
            Bt_Turn.Background = Blanc;
            Bt_Rand_T.Background = Blanc;
            // 🔥 Etat actif
            Bt_Rand_R.Background = Orange;
        }
        // Révèle le Main Flop Turn River
        private void Bt_Rand_MTFR_Click(object sender, RoutedEventArgs e)
        {
            Bt_J_ai_Gagne.Background = (Brush)FindResource("blanc");

            InitialiserPaquet();
            // =========================================================
            // 🔥 RESET PRE-FLOP
            // =========================================================
            bPreFlop_Paire = false;
            bPreFlop_Pareille = false;
            bPreFlop_Suite = false;

            Bt_PreFlop_Paire.Background = Blanc;
            Bt_PreFlop_Pareille.Background = Blanc;
            Bt_PreFlop_Suite.Background = Blanc;
            // =========================================================
            // 🔥 RESET FLOP
            // =========================================================
            bFlop_Paire = false;
            bFlop_2_Paires = false;
            bFlop_Brelan = false;
            bFlop_Quinte = false;
            bFlop_Couleur = false;
            bFlop_Full = false;
            bFlop_Carre = false;
            bFlop_QFlush = false;

            Bt_Flop_Paire.Background = Blanc;
            Bt_Flop_2_Paires.Background = Blanc;
            Bt_Flop_Brelan.Background = Blanc;
            Bt_Flop_Quinte.Background = Blanc;
            Bt_Flop_Couleur.Background = Blanc;
            Bt_Flop_Full.Background = Blanc;
            Bt_Flop_Carre.Background = Blanc;
            Bt_Flop_Quinte_Flush.Background = Blanc;
            // =========================================================
            // 🔥 RESET TURN
            // =========================================================
            bTurn_Paire = false;
            bTurn_2_Paires = false;
            bTurn_Brelan = false;
            bTurn_Quinte = false;
            bTurn_Couleur = false;
            bTurn_Full = false;
            bTurn_Carre = false;
            bTurn_QFlush = false;

            Bt_Turn_Paire.Background = Blanc;
            Bt_Turn_2_Paires.Background = Blanc;
            Bt_Turn_Brelan.Background = Blanc;
            Bt_Turn_Quinte.Background = Blanc;
            Bt_Turn_Couleur.Background = Blanc;
            Bt_Turn_Full.Background = Blanc;
            Bt_Turn_Carre.Background = Blanc;
            Bt_Turn_Quinte_Flush.Background = Blanc;
            // =========================================================
            // 🔥 RESET RIVER
            // =========================================================
            bRiver_Paire = false;
            bRiver_2_Paires = false;
            bRiver_Brelan = false;
            bRiver_Quinte = false;
            bRiver_Couleur = false;
            bRiver_Full = false;
            bRiver_Carre = false;
            bRiver_QFlush = false;

            Bt_River_Paire.Background = Blanc;
            Bt_River_2_Paires.Background = Blanc;
            Bt_River_Brelan.Background = Blanc;
            Bt_River_Quinte.Background = Blanc;
            Bt_River_Couleur.Background = Blanc;
            Bt_River_Full.Background = Blanc;
            Bt_River_Carre.Background = Blanc;
            Bt_River_Quinte_Flush.Background = Blanc;
            // =========================================================
            // 🔥 RESET TIRAGE
            // =========================================================

            for (int i = 0; i < 7; i++)
                Tirage[i].Affichee = false;

            // =========================================================
            // 🔥 TIRAGE DES 7 CARTES
            // =========================================================
            for (int i = 0; i < 7; i++)
            {
                Tirage[i].Carte = TirerCarte();
                Tirage[i].Affichee = false;

                T_Carte[i] = ExtraireCarte(Tirage[i].Carte);
            }
            // =========================================================
            // 🔥 AFFICHAGE COMPLET
            // =========================================================
            AfficherDepuisTirage(0, TB_M1, Bt_Main_1);
            AfficherDepuisTirage(1, TB_M2, Bt_Main_2);

            AfficherDepuisTirage(2, TB_F1, Bt_Flop_1);
            AfficherDepuisTirage(3, TB_F2, Bt_Flop_2);
            AfficherDepuisTirage(4, TB_F3, Bt_Flop_3);

            AfficherDepuisTirage(5, TB_T, Bt_Turn);
            AfficherDepuisTirage(6, TB_R, Bt_River);
            // =========================================================
            // 🔥 CALCULS AUTOMATIQUES
            // =========================================================
            //Calcul_PreFlop();
            //Calcul_Flop();
            //Calcul_Tirages_Flop();
            //Calcul_Turn();
            //Calcul_Tirages_Turn();
            //Calcul_River();
            // =========================================================
            // 🔥 RESET BOUTONS CARTES
            // =========================================================

            Bt_Main_1.Background = Blanc;
            Bt_Main_2.Background = Blanc;

            Bt_Flop_1.Background = Blanc;
            Bt_Flop_2.Background = Blanc;
            Bt_Flop_3.Background = Blanc;

            Bt_Turn.Background = Blanc;

            Bt_Rand_M.Background = Blanc;
            Bt_Rand_F.Background = Blanc;
            Bt_Rand_R.Background = Blanc;
            Bt_Rand_T.Background = Blanc;

            // 🔥 River active
            Bt_River.Background = Orange;
        }

        private void ActiverMise(Button boutonActif)
        {
            foreach (var b in BoutonsMise)
                b.Background = (Brush)FindResource("blanc");

            boutonActif.Background = (Brush)FindResource("bleu");

            switch (boutonActif.Name)
            {
                case "Bt_M9": MiseSelectionnee = -9; break;
                case "Bt_M8": MiseSelectionnee = -8; break;
                case "Bt_M6": MiseSelectionnee = -6; break;
                case "Bt_M4": MiseSelectionnee = -4; break;
                case "Bt_M2": MiseSelectionnee = -2; break;

                case "Bt_0": MiseSelectionnee = 0; break;

                case "Bt_P2": MiseSelectionnee = 2; break;
                case "Bt_P4": MiseSelectionnee = 4; break;
                case "Bt_P6": MiseSelectionnee = 6; break;
                case "Bt_P8": MiseSelectionnee = 8; break;
                case "Bt_P10": MiseSelectionnee = 10; break;
            }
        }
        private void BasculerMainDansRangeEtMettreAJourBouton(string main, Button bouton)
        {
            // =====================================
            // RANGE ACTIF
            // =====================================
            HashSet<string> rangeActif;

            switch (RangeActif)
            {
                case TypeRange.Tapis:
                    rangeActif = RangeTapis;
                    break;

                case TypeRange.Relance:
                    rangeActif = RangeRelance;
                    break;

                case TypeRange.Suivre:
                    rangeActif = RangeSuivre;
                    break;

                default:
                    return;
            }

            // =====================================
            // BASCULER LA MAIN
            // =====================================
            if (rangeActif.Contains(main))
            {
                rangeActif.Remove(main);

                bouton.Background =
                    (Brush)FindResource("blanc");
            }
            else
            {
                rangeActif.Add(main);

                bouton.Background =
                    (Brush)FindResource("gold");
            }
        }
        private void Bt_Main_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string main = btn.Content.ToString();
                BasculerMainDansRangeEtMettreAJourBouton(main, btn);
            }
        }
        private void Bt_J_ai_Gagne_Click(object sender, RoutedEventArgs e)
        {
            if (Bt_J_ai_Gagne.Background.Equals((Brush)FindResource("vert1")))
                return;

            Bt_J_ai_Gagne.Background = (Brush)FindResource("vert1");
            NbTapisGagne++;

            // 🔥 reset UNIQUEMENT si Auto actif ET Se_Recare actif
            if (autoActif && Bt_Se_Recave.Background.Equals((Brush)FindResource("bleu")))
            {
                niveauMise = niveauMemoAuto;
                AppliquerNiveauMise();
            }
        }

        private void Bt_Joueur1_Fold_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur2_Fold_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur3_Fold_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur4_Fold_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur5_Fold_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur6_Fold_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur7_Fold_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur8_Fold_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur2_Tapis_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur9_Fold_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur3_Tapis_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur4_Tapis_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur5_Tapis_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur6_Tapis_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur7_Tapis_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur8_Tapis_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur9_Tapis_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Gagne_en_1_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Gagne_en_2_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Gagne_en_3_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Gagne_en_4_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Gagne_en_5_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Gagne_en_6_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Gagne_en_7_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Gagne_en_8_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Gagne_en_9_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Force_Parole_Passe_Click(object sender, RoutedEventArgs e) { }

        private void Bt_J_ai_Fais_Tapis_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Je_Fais_Tapis_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Bouton_Click(object sender, RoutedEventArgs e) { }

        private void Bt_S_Click(object sender, RoutedEventArgs e) { }

        private void tB_Mise_Coeff_TextChanged(object sender, TextChangedEventArgs e) { }

        private void tB_Mise_TextChanged(object sender, TextChangedEventArgs e) { }

        private void Bt_PreFlop_Paire_Click(object sender, RoutedEventArgs e) { }

        private void Bt_PreFlop_Pareille_Click(object sender, RoutedEventArgs e) { }

        private void Bt_PreFlop_Suite_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Flop_Paire_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Flop_2_Paires_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Flop_Brelan_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Flop_Quinte_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Flop_Couleur_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Flop_Full_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Flop_Carre_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Flop_Quinte_Flush_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Turn_Paire_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Turn_2_Paires_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Turn_Brelan_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Turn_Quinte_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Turn_Couleur_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Turn_Full_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Turn_Carre_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Turn_Quinte_Flush_Click(object sender, RoutedEventArgs e) { }

        private void Bt_River_Paire_Click(object sender, RoutedEventArgs e) { }

        private void Bt_River_2_Paires_Click(object sender, RoutedEventArgs e) { }

        private void Bt_River_Brelan_Click(object sender, RoutedEventArgs e) { }

        private void Bt_River_Quinte_Click(object sender, RoutedEventArgs e) { }

        private void Bt_River_Couleur_Click(object sender, RoutedEventArgs e) { }

        private void Bt_River_Full_Click(object sender, RoutedEventArgs e) { }

        private void Bt_River_Carre_Click(object sender, RoutedEventArgs e) { }

        private void Bt_River_Quinte_Flush_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Tir_Flop_Quinte_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Tir_Flop_Couleur_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Tir_Flop_Quinte_Flush_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Tir_Turn_Quinte_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Tir_Turn_Couleur_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Tir_Turn_Quinte_Flush_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Passe_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Parole_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Relance_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Joueur1_Tapis_Click(object sender, RoutedEventArgs e) { }

        private void boutonRAZ_Click(object sender, RoutedEventArgs e) { }

        private void Bt_Pas_de_Bouton_Click(object sender, RoutedEventArgs e)
        {
            Bt_Pas_de_Bouton.Background = (Brush)FindResource("jaune");
            Bt_Bouton_en_1.Background = (Brush)FindResource("blanc");
            Bt_Bouton_en_2.Background = (Brush)FindResource("blanc");
            Bt_Bouton_en_3.Background = (Brush)FindResource("blanc");
            Bt_Bouton_en_4.Background = (Brush)FindResource("blanc");
            Bt_Bouton_en_5.Background = (Brush)FindResource("blanc");
            Bt_Bouton_en_6.Background = (Brush)FindResource("blanc");
            Bt_Bouton_en_7.Background = (Brush)FindResource("blanc");
            Bt_Bouton_en_8.Background = (Brush)FindResource("blanc");
            Bt_Bouton_en_9.Background = (Brush)FindResource("blanc");
        }

        // =========================================================
        // 🔥 CONVERTIT "A♠" EN CARTE
        // =========================================================
        private Carte CreerCarte(string texteCarte, bool estMain)
        {
            string ordre = "23456789TJQKA";

            string valeurTexte = texteCarte.StartsWith("10")
                ? "T"
                : texteCarte.Substring(0, texteCarte.Length - 1);

            return new Carte
            {
                Valeur = ordre.IndexOf(valeurTexte) + 2,
                Couleur = texteCarte[texteCarte.Length - 1],
                EstMain = estMain
            };
        }
        private void Bt_Pause_Click(object sender, RoutedEventArgs e)
        {
            JeuEnPause = true;

            //chrono.Stop(); // pause chrono

            this.WindowState = WindowState.Minimized;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                JeuEnPause = true;
                DebutPause = DateTime.Now;

                timer.Stop(); // 🔥 pause
            }
            else if (this.WindowState == WindowState.Normal)
            {
                if (JeuEnPause)
                {
                    JeuEnPause = false;

                    // 🔥 corrige le temps (important)
                    DebutSession += (DateTime.Now - DebutPause);

                    timer.Start(); // 🔥 reprise
                }
            }
        }

        private void ActiverBoutonPosition(Button boutonActif)
        {
            foreach (var b in BoutonsBoutonPosition)
                b.Background = (Brush)FindResource("blanc");

            boutonActif.Background = (Brush)FindResource("jaune");
            Bt_Pas_de_Bouton.Background = (Brush)FindResource("blanc");
        }

        private void Bt_Bouton_en_1_Click(object sender, RoutedEventArgs e)
        {
            ActiverBoutonPosition((Button)sender);
            Bt_Pas_de_Bouton.Background = (Brush)FindResource("blanc");
        }

        private void Bt_Bouton_en_2_Click(object sender, RoutedEventArgs e)
        {
            ActiverBoutonPosition((Button)sender);
        }

        private void Bt_Bouton_en_3_Click(object sender, RoutedEventArgs e)
        {
            ActiverBoutonPosition((Button)sender);
        }

        private void Bt_Bouton_en_4_Click(object sender, RoutedEventArgs e)
        {
            ActiverBoutonPosition((Button)sender);
        }

        private void Bt_Bouton_en_5_Click(object sender, RoutedEventArgs e)
        {
            ActiverBoutonPosition((Button)sender);
        }

        private void Bt_Bouton_en_6_Click(object sender, RoutedEventArgs e)
        {
            ActiverBoutonPosition((Button)sender);
        }

        private void Bt_Bouton_en_7_Click(object sender, RoutedEventArgs e)
        {
            ActiverBoutonPosition((Button)sender);
        }

        private void Bt_Bouton_en_8_Click(object sender, RoutedEventArgs e)
        {
            ActiverBoutonPosition((Button)sender);
        }

        private void Bt_Bouton_en_9_Click(object sender, RoutedEventArgs e)
        {
            ActiverBoutonPosition((Button)sender);
        }

        private void Bt_Position_1_Click(object sender, RoutedEventArgs e)
        {
            ActiverPosition((Button)sender);
        }

        private void Bt_Position_2_Click(object sender, RoutedEventArgs e)
        {
            ActiverPosition((Button)sender);
        }

        private void Bt_Position_3_Click(object sender, RoutedEventArgs e)
        {
            ActiverPosition((Button)sender);
        }

        private void Bt_Position_4_Click(object sender, RoutedEventArgs e)
        {
            ActiverPosition((Button)sender);
        }

        private void Bt_Position_5_Click(object sender, RoutedEventArgs e)
        {
            ActiverPosition((Button)sender);
        }

        private void Bt_Position_6_Click(object sender, RoutedEventArgs e)
        {
            ActiverPosition((Button)sender);
        }

        private void Bt_Position_7_Click(object sender, RoutedEventArgs e)
        {
            ActiverPosition((Button)sender);
        }
        private void Bt_Position_8_Click(object sender, RoutedEventArgs e)
        {
            ActiverPosition((Button)sender);
        }

        private void Bt_Position_9_Click(object sender, RoutedEventArgs e)
        {
            ActiverPosition((Button)sender);
        }

        private void RafraichirAffichage()
        {
            foreach (var kvp in BoutonsRange)
            {
                string main = kvp.Key;
                Button bouton = kvp.Value;

                if (EstDansRange(main))
                    bouton.Background = (Brush)FindResource("gold");
                else
                    bouton.Background = (Brush)FindResource("blanc");
            }
        }

        private void Bt_Mise_Click(object sender, RoutedEventArgs e)
        {
            ActiverMise((Button)sender);
        }

        private void Bt_Joueur_Mise_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                BasculerEtatBouton(btn, (Brush)FindResource("jaune"));
            }
        }

        private void CbRandom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MettreAJourIndicateurRandom();
        }

        private void Bt_Je_suis_a_table_Click(object sender, RoutedEventArgs e)
        {
            if (Bt_Je_suis_a_table.Background.Equals((Brush)FindResource("blanc")))
            {
                Bt_Je_suis_a_table.Background = (Brush)FindResource("vert");
            }
            else
            {
                Bt_Je_suis_a_table.Background = (Brush)FindResource("blanc");
            }
        }

        // Methode de basculement des couleurs pour Bt_Un_Joueur gold && Bt_PaceDisponible vert
        private void BasculerEtatBouton(Button bouton, Brush couleurActive)
        {
            Brush blanc = (Brush)FindResource("blanc");

            if (bouton.Background == blanc)
                bouton.Background = couleurActive;
            else
                bouton.Background = blanc;
        }

        private void Bt_PlaceDisponible_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                BasculerEtatBouton(btn, (Brush)FindResource("vert"));
            }
        }

        private void Bt_Auto_M9_Click(object sender, RoutedEventArgs e)
        {
            ActiverAutoMise((Button)sender);
        }

        private void Bt_Auto_M8_Click(object sender, RoutedEventArgs e)
        {
            ActiverAutoMise((Button)sender);
        }

        private void Bt_Auto_M6_Click(object sender, RoutedEventArgs e)
        {
            ActiverAutoMise((Button)sender);
        }

        private void Bt_Auto_M4_Click(object sender, RoutedEventArgs e)
        {
            ActiverAutoMise((Button)sender);
        }

        private void Bt_Auto_M2_Click(object sender, RoutedEventArgs e)
        {
            ActiverAutoMise((Button)sender);
        }

        private void Bt_Auto_0_Click(object sender, RoutedEventArgs e)
        {
            ActiverAutoMise((Button)sender);
        }

        private void Bt_Auto_P2_Click(object sender, RoutedEventArgs e)
        {
            ActiverAutoMise((Button)sender);
        }

        private void Bt_Auto_P4_Click(object sender, RoutedEventArgs e)
        {
            ActiverAutoMise((Button)sender);
        }

        private void Bt_Auto_P6_Click(object sender, RoutedEventArgs e)
        {
            ActiverAutoMise((Button)sender);
        }

        private void Bt_Auto_P8_Click(object sender, RoutedEventArgs e)
        {
            ActiverAutoMise((Button)sender);
        }

        private void Bt_Un_Joueur_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                BasculerEtatBouton(btn, (Brush)FindResource("gold"));
            }
        }

        private void Bt_Bouton_en_Click(object sender, RoutedEventArgs e)
        {
            ActiverBoutonPosition((Button)sender);
        }

        private string GetMain()
        {
            if (string.IsNullOrEmpty(TB_M1.Text) || string.IsNullOrEmpty(TB_M2.Text))
                return null;

            string c1 = TB_M1.Text;
            string c2 = TB_M2.Text;

            string v1 = c1.Substring(0, c1.Length - 1);
            string v2 = c2.Substring(0, c2.Length - 1);

            char s1 = c1[c1.Length - 1];
            char s2 = c2[c2.Length - 1];

            string ordre = "AKQJ1098765432";
            // position dans l'ordre
            int i1 = ordre.IndexOf(v1);
            int i2 = ordre.IndexOf(v2);
            // mettre la plus forte en premier
            string high = i1 < i2 ? v1 : v2;
            string low = i1 < i2 ? v2 : v1;
            // paire (AA, KK, etc.)
            if (v1 == v2)
                return high + low;
            // suited ?
            if (s1 == s2)
                return high + low + "p";
            // off
            return high + low;
        }

        private void EvaluerTapisDepuisTextBox()
        {
            string brute = GetMain();
            // 🛡️ sécurité
            if (string.IsNullOrWhiteSpace(brute))
            {
                Bt_Tapis.Background = Blanc;
                return;
            }
            // 🔥 NORMALISATION
            string main = NormaliserMainFromString(brute);

            // 🛡️ sécurité normalisation
            if (string.IsNullOrWhiteSpace(main))
            {
                Bt_Tapis.Background = Blanc;
                return;
            }
            // 🎯 décision
            if (RangeTapis != null && RangeTapis.Contains(main))
                Bt_Tapis.Background = Vert;
            else
                Bt_Tapis.Background = Blanc;
        }

        private void Bt_Tapis_Click(object sender, RoutedEventArgs e)
        {
            ActionForcee = "TAPIS";
            AppliquerActionForcee();
        }

        private void Bt_Parole_Passe_Click(object sender, RoutedEventArgs e)
        {
            ActionForcee = "PAROLE";

            AppliquerActionForcee();
        }

        private void Bt_Suivre_Click(object sender, RoutedEventArgs e)
        {
            ActionForcee = "SUIVRE";
            AppliquerActionForcee();
        }

        private void Bt_Relancer_Click(object sender, RoutedEventArgs e)
        {
            ActionForcee = "RELANCER";
            AppliquerActionForcee();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 🔥 STOP TOTAL si pause
            if (JeuEnPause)
                return;

            TimeSpan duree = DateTime.Now - DebutSession;

            double mainsParHeure = 0;

            if (duree.TotalHours > 0)
                mainsParHeure = NbMains / duree.TotalHours;

            LbTemps.Content = duree.ToString(@"hh\:mm\:ss");
            LbNbParole.Content = NbMains.ToString();
            LbNbTapis.Content = NbTapis.ToString();
            LbMH.Content = $"Main/h: {mainsParHeure:0}";
        }

        private void ActiverAutoMise(Button boutonActif)
        {
            foreach (var b in BoutonsAutoMise)
                b.Background = (Brush)FindResource("blanc");

            boutonActif.Background = (Brush)FindResource("bleu");

            // 🔥 mémoire uniquement
            niveauMemoAuto = BoutonsAutoMise.IndexOf(boutonActif);

            // ❌ NE JAMAIS TOUCHER :
            // niveauMise = ...
        }

        private void AppliquerNiveauMise()
        {
            // reset tous les boutons de mise
            foreach (var b in NiveauxMise)
                b.Background = (Brush)FindResource("blanc");

            // activer le niveau courant en BLEU (important pour ton script)
            NiveauxMise[niveauMise].Background = (Brush)FindResource("bleu");
        }

        private void Bt_Auto_GO_Click(object sender, RoutedEventArgs e)
        {
            FlashGO(); // 🔥 effet visuel immédiat

            if (!autoActif)
                return;

            // 🔍 synchro avec bouton bleu
            for (int i = 0; i < NiveauxMise.Count; i++)
            {
                if (NiveauxMise[i].Background == (Brush)FindResource("bleu"))
                {
                    niveauMise = i;
                    break;
                }
            }

            // 🔼 progression
            if (niveauMise < NiveauxMise.Count - 1)
                niveauMise++;

            AppliquerNiveauMise();
        }

        private void Bt_Auto_Click(object sender, RoutedEventArgs e)
        {
            if (Bt_Se_Recave.Background.Equals((Brush)FindResource("blanc")))
                return;
            autoActif = !autoActif;

            Bt_Auto.Background = autoActif
                ? (Brush)FindResource("vert")
                : (Brush)FindResource("blanc");
        }

        async void FlashGO()
        {
            Bt_Auto_GO.Background = (Brush)FindResource("vert");
            await Task.Delay(120); // durée du flash (ms)
            Bt_Auto_GO.Background = (Brush)FindResource("blanc");
        }
        // =========================================================
        // 🔥 CLIGNOTEMENT BOUTON DANGER
        // =========================================================
        private bool bFlashDanger = false;

        private async void FlashDanger()
        {
            if (bFlashDanger)
                return;

            bFlashDanger = true;

            while (bFlashDanger)
            {
                // =====================================
                // ROUGE
                // =====================================
                Bt_Danger.Background = Brushes.Red;
                Bt_Danger.Foreground = Brushes.White;

                await Task.Delay(250);

                // =====================================
                // BLANC
                // =====================================
                Bt_Danger.Background = Brushes.White;
                Bt_Danger.Foreground = Brushes.Red;

                await Task.Delay(250);
            }
        }

        // =========================================================
        // 🔥 ARRÊTER CLIGNOTEMENT DANGER
        // =========================================================
        private void StopFlashDanger()
        {
            bFlashDanger = false;

            Bt_Danger.Background = Blanc;
            Bt_Danger.Foreground = Brushes.Black;
        }

        private string NormaliserMain(string carte1, string carte2)
        {
            if (string.IsNullOrWhiteSpace(carte1) ||
                string.IsNullOrWhiteSpace(carte2))
                return null;

            if (carte1.Length < 2 || carte2.Length < 2)
                return null;

            string v1 = carte1.Substring(0, carte1.Length - 1);
            string c1 = carte1.Substring(carte1.Length - 1);

            string v2 = carte2.Substring(0, carte2.Length - 1);
            string c2 = carte2.Substring(carte2.Length - 1);

            string ordre = "AKQJ1098765432";

            if (!ordre.Contains(v1) || !ordre.Contains(v2))
                return null;

            if (ordre.IndexOf(v2) < ordre.IndexOf(v1))
            {
                var tmpV = v1;
                v1 = v2;
                v2 = tmpV;

                var tmpC = c1;
                c1 = c2;
                c2 = tmpC;
            }

            if (v1 == v2)
                return v1 + v2;

            bool suited = (c1 == c2);

            return suited
                ? v1 + v2 + "p"
                : v1 + v2;
        }


        private string NormaliserMainFromString(string brute)
        {
            if (string.IsNullOrWhiteSpace(brute))
                return null;

            brute = brute.Replace(" ", "");

            List<string> cartes = new List<string>();

            int i = 0;

            while (i < brute.Length)
            {
                string valeur;

                if (i + 1 < brute.Length &&
                    brute[i] == '1' &&
                    brute[i + 1] == '0')
                {
                    valeur = "10";
                    i += 2;
                }
                else
                {
                    valeur = brute[i].ToString();
                    i += 1;
                }

                if (i >= brute.Length)
                    return null;

                string couleur = brute[i].ToString();
                i += 1;

                cartes.Add(valeur + couleur);
            }

            if (cartes.Count < 2)
                return null;

            return NormaliserMain(
                cartes[0],
                cartes[1]);
        }


        private bool EstDansRange(string main)
        {
            if (string.IsNullOrWhiteSpace(main))
                return false;

            switch (RangeActif)
            {
                case TypeRange.Tapis:
                    return RangeTapis.Contains(main);

                case TypeRange.Relance:
                    return RangeRelance.Contains(main);

                case TypeRange.Suivre:
                    return RangeSuivre.Contains(main);

                default:
                    return false;
            }
        }

        private bool Detecter_Presence(string nom)
        {
            for (int i = 0; i < MainBoardDetect.Length; i++)
            {
                // =====================================
                // Nom différent
                // =====================================

                if (MainBoardDetect[i].Nom != nom)
                    continue;

                // =====================================
                // Lecture pixel
                // =====================================

                int couleur = LirePixel(MainBoardDetect[i].X, MainBoardDetect[i].Y);

                // =====================================
                // Couleur trouvée
                // =====================================

                if (couleur ==
                    MainBoardDetect[i].Couleur)
                {
                    return true;
                }
            }

            return false;
        }



        // =========================================================
        // 🔥 INITIALISE
        // =========================================================
        private void Bt_Initialise_Click(object sender, RoutedEventArgs e)
        {
            // =====================================
            // ARRÊTER LE CLIGNOTEMENT DANGER
            // =====================================
            StopFlashDanger();

            // 🔥 Remise à zéro des compteurs
            NbMains = 0;
            NbTapis = 0;
            NbParole = 0;

            // 🔥 Reset du temps
            DebutSession = DateTime.Now;

            // 🔥 Effacement de l'historique
            RTB_Historique.Document.Blocks.Clear();

            // 🔥 Reset affichage
            LbTemps.Content = "00:00:00";
            LbNbParole.Content = "0";
            LbNbTapis.Content = "0";
            LbMH.Content = "Main/h: 0";

            // 🔥 Reset état
            JeuEnPause = false;

            // =====================================================
            // 🔥 RESET TIRAGE FLOP
            // =====================================================
            TirageFlop.Quinte = false;
            TirageFlop.Ventrale = false;
            TirageFlop.Couleur = false;
            TirageFlop.QuinteFlush = false;
            TirageFlop.Participation = 0;
            TirageFlop.Max = false;
            TirageFlop.CarteHaute = 0;

            // =====================================================
            // 🔥 RESET DANGERS FLOP
            // =====================================================
            TirageFlop.DangerQuinte = false;
            TirageFlop.DangerCouleur = false;
            TirageFlop.DangerQuinteFlush = false;

            // =====================================================
            // 🔥 RESET TIRAGE TURN
            // =====================================================
            TirageTurn.Quinte = false;
            TirageTurn.Ventrale = false;
            TirageTurn.Couleur = false;
            TirageTurn.QuinteFlush = false;
            TirageTurn.Participation = 0;
            TirageTurn.Max = false;
            TirageTurn.CarteHaute = 0;

            // =====================================================
            // 🔥 RESET DANGERS TURN
            // =====================================================
            TirageTurn.DangerQuinte = false;
            TirageTurn.DangerCouleur = false;
            TirageTurn.DangerQuinteFlush = false;

            // =====================================================
            // 🔥 RESET AFFICHAGE TIRAGES FLOP
            // =====================================================
            Bt_Tir_Flop_Quinte.Background = Blanc;
            Bt_Tir_Flop_Quinte.Content = "Q";

            Bt_Tir_Flop_Couleur.Background = Blanc;

            // =====================================================
            // 🔥 RESET AFFICHAGE TIRAGES TURN
            // =====================================================
            Bt_Tir_Turn_Quinte.Background = Blanc;
            Bt_Tir_Turn_Quinte.Content = "Q";

            Bt_Tir_Turn_Couleur.Background = Blanc;

            // =====================================================
            // 🔥 RESET AFFICHAGE DANGER
            // =====================================================
            Bt_Danger.Background = Blanc;
            Bt_Danger.Foreground = Brushes.Black;
            Bt_Danger.Content = "";

            // =====================================================
            // 🔥 RESET FLAGS SONS
            // =====================================================
            bPreFlop_Calcul = false;

            bFlop_Calcul = false;
            bFlop_Tirage = false;
            bFlop_Danger = false;

            bTurn_Calcul = false;
            bTurn_Tirage = false;
            bTurn_Danger = false;

            bRiver_Calcul = false;
            bRiver_Danger = false;

            // 🔥 Reset Debug
            TB_Debug.Clear();
            Debug("Lecture Main1");

            // 🔥 Reprise du timer
            if (timer != null)
                timer.Start();

            Reset_Board();
        }

        // =========================================================
        // 🔥 CAPTURE ECRAN
        // =========================================================
        private void CaptureEcran()
        {
            // =====================================
            // Libération ancienne bitmap
            // =====================================

            if (Bitmap_Main != null)
            {
                Bitmap_Main.Dispose();

                Bitmap_Main = null;
            }

            // =====================================
            // Zone écran
            // =====================================

            System.Drawing.Rectangle zone =
                new System.Drawing.Rectangle(
                    0,
                    0,
                    (int)SystemParameters.PrimaryScreenWidth,
                    (int)SystemParameters.PrimaryScreenHeight);

            // =====================================
            // Création bitmap
            // =====================================

            Bitmap_Main =
                new System.Drawing.Bitmap(
                    zone.Width,
                    zone.Height);

            // =====================================
            // Capture
            // =====================================

            using (System.Drawing.Graphics g =
                System.Drawing.Graphics.FromImage(Bitmap_Main))
            {
                g.CopyFromScreen(
                    0,
                    0,
                    0,
                    0,
                    zone.Size);
            }
        }

        // =========================================================
        // 🔥 EXTRAIRE CARTES
        // =========================================================
        private List<string> Extraire_Cartes(string txt)
        {
            List<string> cartes = new List<string>();
            MatchCollection mc = Regex.Matches(txt, @"10[♠♥♦♣]|[2-9AJQKA][♠♥♦♣]");

            foreach (Match m in mc)
            {
                cartes.Add(m.Value);
            }

            return cartes;
        }
        #endregion

        // =========================================================
        // LIRE PIXEL
        // =========================================================
        private int LirePixel(int x, int y)
        {
            // =====================================
            // SECURITE
            // =====================================
            if (Bitmap_Main == null)
                return 0;

            // =====================================
            // LECTURE PIXEL
            // =====================================
            System.Drawing.Color c =
                Bitmap_Main.GetPixel(x, y);

            // =====================================
            // RGB 24 bits
            // =====================================
            return c.ToArgb() & 0xFFFFFF;
        }

        #region JOUER SONS
        // =====================================
        // 🔥 JOUER UN SON
        // =====================================
        private void JouerSon(string nomSon)
        {

            return;
            // =====================================
            // SON INVALIDE
            // =====================================
            if (string.IsNullOrWhiteSpace(nomSon))
                return;

            // =====================================
            // CONSTRUIRE LE CHEMIN
            // =====================================
            string fichier = Path.Combine(DOSSIER_SONS, nomSon);

            // =====================================
            // FICHIER ABSENT
            // =====================================
            if (!File.Exists(fichier))
                return;

            // =====================================
            // JOUER LE SON
            // =====================================
            JouerFichierSon(fichier);
        }

        // =========================================================
        // 🔥 JOUER UN FICHIER SON
        // =========================================================
        private void JouerFichierSon(string fichier)
        {
            try
            {
                string chemin = Path.Combine("Sons", fichier);

                if (!File.Exists(chemin))
                    return;

                PlaySound(chemin, IntPtr.Zero, SND_ASYNC | SND_FILENAME | SND_NODEFAULT);
            }
            catch
            {
            }
        }
        #endregion

        #region CALCUL MOTEUR
        // =========================================================
        // 🔥 PAREILLE
        // =========================================================
        private void Calcul_Moteur_Pareille(List<Carte> cartes, ref bool bPareille, Button bouton)
        {
            bPareille = false;

            if (cartes.Count < 2)
                return;

            if (cartes[0].Couleur == cartes[1].Couleur)
            {
                bPareille = true;
                bouton.Background = Vert;
            }
        }
        // =========================================================
        // 🔥 SUITE
        // =========================================================
        private void Calcul_Moteur_Suite(List<Carte> cartes, ref bool bSuite, Button bouton)
        {
            bSuite = false;

            if (cartes.Count < 2)
                return;

            int v1 = cartes[0].Valeur;
            int v2 = cartes[1].Valeur;

            if (Math.Abs(v1 - v2) == 1 ||
                (v1 == 14 && v2 == 2) ||
                (v1 == 2 && v2 == 14))
            {
                bSuite = true;
                bouton.Background = Vert;
            }
        }
        // =========================================================
        // 🔥 PAIRE
        // =========================================================
        private void Calcul_Moteur_Paire(List<Carte> cartes, Jeu jeu, ref bool bPaire, Button bouton)
        {
            // RESET
            bPaire = false;

            jeu.Force = 0;
            jeu.Qualite = 0;
            jeu.Participation = 0;

            // RECHERCHE D'UNE PAIRE
            var groupe = cartes
                .GroupBy(c => c.Valeur)
                .FirstOrDefault(g => g.Count() == 2);

            if (groupe == null)
                return;

            // =====================================
            // PAS UNE SIMPLE PAIRE
            // (Brelan ou Carré)
            // =====================================
            if (cartes
                .GroupBy(c => c.Valeur)
                .Any(g => g.Count() >= 3))
                return;

            // =====================================
            // PAIRE TROUVEE
            // =====================================
            bPaire = true;

            jeu.Force = Math.Max(jeu.Force, 1);

            // Valeur de la paire
            jeu.Qualite = groupe.Key;

            // Nombre de cartes de ma main
            jeu.Participation = groupe.Count(c => c.EstMain);

            // Affichage
            bouton.Background = Vert;
        }

        // =========================================================
        // 🔥 2 PAIRES
        // Peut gérer 2 ou 3 paires
        // Conserve toujours les 2 plus fortes
        // =========================================================
        private void Calcul_Moteur_DeuxPaires(List<Carte> cartes, Jeu jeu, ref bool bDeuxPaires, Button bouton)
        {
            // =====================================
            // RESET
            // =====================================
            bDeuxPaires = false;

            // =====================================
            // RECHERCHE DES PAIRES
            // =====================================
            var paires = cartes
                .GroupBy(c => c.Valeur)
                .Where(g => g.Count() >= 2)
                .OrderByDescending(g => g.Key)
                .Take(2)
                .ToList();

            // =====================================
            // IL FAUT AU MOINS 2 PAIRES
            // =====================================
            if (paires.Count < 2)
                return;

            // =====================================
            // 2 PAIRES TROUVEES
            // =====================================
            bDeuxPaires = true;

            jeu.Force = Math.Max(jeu.Force, 2);

            // =====================================
            // QUALITE = PLUS FORTE DES 2 PAIRES
            // =====================================
            jeu.Qualite = paires[0].Key;

            // =====================================
            // PARTICIPATION
            // UNIQUEMENT LES 2 PAIRES RETENUES
            // =====================================
            jeu.Participation =
                paires.Sum(g => g.Count(c => c.EstMain));

            // =====================================
            // AFFICHAGE
            // =====================================
            bouton.Background = Vert;
        }

        // =========================================================
        // 🔥 BRELAN
        // =========================================================
        private void Calcul_Moteur_Brelan(List<Carte> cartes, Jeu jeu, ref bool bBrelan, Button bouton)
        {
            // RESET
            bBrelan = false;

            // RECHERCHE DU BRELAN
            var brelan = cartes
                .GroupBy(c => c.Valeur)
                .FirstOrDefault(g => g.Count() == 3);

            if (brelan == null)
                return;

            // Pas un Full
            if (cartes
                .GroupBy(c => c.Valeur)
                .Any(g => g.Count() == 2))
                return;

            // =====================================
            // BRELAN TROUVE
            // =====================================
            bBrelan = true;

            jeu.Force = Math.Max(jeu.Force, 3);

            // Valeur du brelan
            jeu.Qualite = brelan.Key;

            // Nombre de cartes de ma main
            jeu.Participation =
                brelan.Count(c => c.EstMain);

            bouton.Background = Vert;
        }
        // =========================================================
        // 🔥 QUINTE
        // =========================================================
        private void Calcul_Moteur_Quinte(List<Carte> cartes, Jeu jeu, ref bool bQuinte, Button bouton)
        {
            switch (cartes.Count)
            {
                case 5:
                    Calcul_Moteur_Quinte_SansDoublon(cartes, jeu, ref bQuinte, bouton);
                    break;

                case 6:
                    Calcul_Moteur_Quinte_Turn(cartes, jeu, ref bQuinte, bouton);
                    break;

                case 7:
                    Calcul_Moteur_Quinte_River(cartes, jeu, ref bQuinte, bouton);
                    break;
            }
        }
        // =========================================================
        // 🔥 QUINTE TURN (1 PAIRE)
        // =========================================================
        private void Calcul_Moteur_Quinte_Turn(List<Carte> cartes, Jeu jeu, ref bool bQuinte, Button bouton)
        {
            // =====================================
            // RECHERCHE D'UN DOUBLON
            // =====================================
            bool bDoublon = cartes
                .GroupBy(c => c.Valeur)
                .Any(g => g.Count() > 1);

            // =====================================
            // AUCUN DOUBLON
            // =====================================
            if (!bDoublon)
            {
                Calcul_Moteur_Quinte_SansDoublon(cartes, jeu, ref bQuinte, bouton);
                return;
            }

            // =====================================
            // TRI
            // =====================================
            List<Carte> cartesTriees = cartes
                .OrderBy(c => c.Valeur)
                .ToList();

            // =====================================
            // RECHERCHE DE LA PAIRE
            // =====================================
            int indicePaire = -1;

            for (int i = 0; i < cartesTriees.Count - 1; i++)
            {
                if (cartesTriees[i].Valeur == cartesTriees[i + 1].Valeur)
                {
                    indicePaire = i;
                    break;
                }
            }

            if (indicePaire == -1)
                return;

            // =====================================
            // CHEMIN 1
            // On saute la 2ème carte de la paire
            // =====================================
            List<Carte> chemin1 = new List<Carte>();

            for (int i = 0; i < cartesTriees.Count; i++)
            {
                if (i == indicePaire + 1)
                    continue;

                chemin1.Add(cartesTriees[i]);
            }

            // =====================================
            // CHEMIN 2
            // On saute la 1ère carte de la paire
            // =====================================
            List<Carte> chemin2 = new List<Carte>();

            for (int i = 0; i < cartesTriees.Count; i++)
            {
                if (i == indicePaire)
                    continue;

                chemin2.Add(cartesTriees[i]);
            }

            // =====================================
            // CALCUL CHEMIN 1
            // =====================================
            Jeu jeu1 = new Jeu();
            bool bQuinte1 = false;

            Calcul_Moteur_Quinte_SansDoublon(
                chemin1,
                jeu1,
                ref bQuinte1,
                bouton);

            // =====================================
            // CALCUL CHEMIN 2
            // =====================================
            Jeu jeu2 = new Jeu();
            bool bQuinte2 = false;

            Calcul_Moteur_Quinte_SansDoublon(
                chemin2,
                jeu2,
                ref bQuinte2,
                bouton);



            // =====================================
            // CONSERVATION DU MEILLEUR RESULTAT
            // =====================================
            if (bQuinte1)
            {
                bQuinte = true;

                jeu.Force = jeu1.Force;
                jeu.Qualite = jeu1.Qualite;
                jeu.Participation = jeu1.Participation;
                jeu.EstQuinteFlush = jeu1.EstQuinteFlush;

                jeu.CartesQuinte.Clear();
                jeu.CartesQuinte.AddRange(jeu1.CartesQuinte);
            }

            if (bQuinte2 && (!bQuinte || jeu2.Qualite > jeu.Qualite))
            {
                bQuinte = true;

                jeu.Force = jeu2.Force;
                jeu.Qualite = jeu2.Qualite;
                jeu.Participation = jeu2.Participation;
                jeu.EstQuinteFlush = jeu2.EstQuinteFlush;

                jeu.CartesQuinte.Clear();
                jeu.CartesQuinte.AddRange(jeu2.CartesQuinte);
            }
        }

        // =========================================================
        // 🔥 QUINTE RIVER
        // =========================================================
        private void Calcul_Moteur_Quinte_River(List<Carte> cartes, Jeu jeu, ref bool bQuinte, Button bouton)
        {
            // =====================================
            // RECHERCHE D'UN DOUBLON
            // =====================================
            bool bDoublon = cartes
                .GroupBy(c => c.Valeur)
                .Any(g => g.Count() > 1);

            // =====================================
            // AUCUN DOUBLON
            // =====================================
            if (!bDoublon)
            {
                Calcul_Moteur_Quinte_SansDoublon(
                    cartes,
                    jeu,
                    ref bQuinte,
                    bouton);

                return;
            }

            // =====================================
            // NOMBRE DE PAIRES / BRELANS
            // =====================================
            int nbPaires = cartes
                .GroupBy(c => c.Valeur)
                .Count(g => g.Count() == 2);

            int nbBrelans = cartes
                .GroupBy(c => c.Valeur)
                .Count(g => g.Count() == 3);

            // =====================================
            // 1 PAIRE
            // =====================================
            if (nbPaires == 1)
            {
                Calcul_Moteur_Quinte_River_1Paire(
                    cartes,
                    jeu,
                    ref bQuinte,
                    bouton);

                return;
            }

            // =====================================
            // 1 BRELAN
            // =====================================
            if (nbBrelans == 1)
            {
                Calcul_Moteur_Quinte_River_1Brelan(cartes, jeu, ref bQuinte, bouton);

                return;
            }

            // =====================================
            // 2 PAIRES
            // =====================================
            Calcul_Moteur_Quinte_River_2Paires(cartes, jeu, ref bQuinte, bouton);
        }

        // =========================================================
        // 🔥 QUINTE RIVER (1 PAIRE)
        // =========================================================
        private void Calcul_Moteur_Quinte_River_1Paire(List<Carte> cartes, Jeu jeu, ref bool bQuinte, Button bouton)
        {
            // =====================================
            // RECHERCHE D'UN DOUBLON
            // =====================================
            bool bDoublon = cartes
                .GroupBy(c => c.Valeur)
                .Any(g => g.Count() > 1);

            // =====================================
            // AUCUN DOUBLON
            // =====================================
            if (!bDoublon)
            {
                Calcul_Moteur_Quinte_SansDoublon(cartes, jeu, ref bQuinte, bouton);
                return;
            }

            // =====================================
            // TRI
            // =====================================
            List<Carte> cartesTriees = cartes
                .OrderBy(c => c.Valeur)
                .ToList();

            // =====================================
            // RECHERCHE DE LA PAIRE
            // =====================================
            int indicePaire = -1;

            for (int i = 0; i < cartesTriees.Count - 1; i++)
            {
                if (cartesTriees[i].Valeur == cartesTriees[i + 1].Valeur)
                {
                    indicePaire = i;
                    break;
                }
            }

            if (indicePaire == -1)
                return;

            // =====================================
            // CHEMIN 1
            // On saute la 2ème carte de la paire
            // =====================================
            List<Carte> chemin1 = new List<Carte>();

            for (int i = 0; i < cartesTriees.Count; i++)
            {
                if (i == indicePaire + 1)
                    continue;

                chemin1.Add(cartesTriees[i]);
            }

            // =====================================
            // CHEMIN 2
            // On saute la 1ère carte de la paire
            // =====================================
            List<Carte> chemin2 = new List<Carte>();

            for (int i = 0; i < cartesTriees.Count; i++)
            {
                if (i == indicePaire)
                    continue;

                chemin2.Add(cartesTriees[i]);
            }

            // =====================================
            // CALCUL CHEMIN 1
            // =====================================
            Jeu jeu1 = new Jeu();
            bool bQuinte1 = false;

            Calcul_Moteur_Quinte_SansDoublon(chemin1, jeu1, ref bQuinte1, bouton);

            // =====================================
            // CALCUL CHEMIN 2
            // =====================================
            Jeu jeu2 = new Jeu();
            bool bQuinte2 = false;

            Calcul_Moteur_Quinte_SansDoublon(chemin2, jeu2, ref bQuinte2, bouton);



            // =====================================
            // CONSERVATION DU MEILLEUR RESULTAT
            // =====================================
            if (bQuinte1)
            {
                bQuinte = true;

                jeu.Force = jeu1.Force;
                jeu.Qualite = jeu1.Qualite;
                jeu.Participation = jeu1.Participation;
                jeu.EstQuinteFlush = jeu1.EstQuinteFlush;

                jeu.CartesQuinte.Clear();
                jeu.CartesQuinte.AddRange(jeu1.CartesQuinte);
            }

            if (bQuinte2 && (!bQuinte || jeu2.Qualite > jeu.Qualite))
            {
                bQuinte = true;

                jeu.Force = jeu2.Force;
                jeu.Qualite = jeu2.Qualite;
                jeu.Participation = jeu2.Participation;
                jeu.EstQuinteFlush = jeu2.EstQuinteFlush;

                jeu.CartesQuinte.Clear();
                jeu.CartesQuinte.AddRange(jeu2.CartesQuinte);
            }

            // =====================================
            // CORRECTION DE LA PARTICIPATION
            // =====================================
            if (bQuinte && jeu.Participation > 0)
            {
                foreach (Carte carteMain in jeu.CartesQuinte.Where(c => c.EstMain))
                {
                    if (cartes.Any(c => !c.EstMain &&
                                        c.Valeur == carteMain.Valeur))
                    {
                        jeu.Participation--;
                    }
                }

                if (jeu.Participation < 0)
                    jeu.Participation = 0;
            }
        }

        // =========================================================
        // 🔥 QUINTE RIVER (2 PAIRES)
        // 4 chemins
        // =========================================================
        private void Calcul_Moteur_Quinte_River_2Paires(List<Carte> cartes, Jeu jeu, ref bool bQuinte, Button bouton)
        {
            // =====================================
            // RECHERCHE DES 2 PAIRES
            // =====================================
            int indiceP1 = -1;
            int indiceP2 = -1;

            // =====================================
            // TRI
            // =====================================
            List<Carte> cartesTriees = cartes
                .OrderBy(c => c.Valeur)
                .ToList();


            for (int i = 0; i < cartesTriees.Count - 1; i++)
            {
                if (cartesTriees[i].Valeur == cartesTriees[i + 1].Valeur)
                {
                    if (indiceP1 == -1)
                        indiceP1 = i;
                    else
                    {
                        indiceP2 = i;
                        break;
                    }

                    i++; // sauter la 2ème carte de la paire
                }
            }

            if (indiceP1 == -1 || indiceP2 == -1)
                return;

            // =====================================
            // CHEMIN 1
            // On saute P1C1 et P2C1
            // =====================================
            List<Carte> chemin1 = new List<Carte>();

            for (int i = 0; i < cartesTriees.Count; i++)
            {
                if (i == indiceP1 || i == indiceP2)
                    continue;

                chemin1.Add(cartesTriees[i]);
            }

            // =====================================
            // CHEMIN 2
            // On saute P1C1 et P2C2
            // =====================================
            List<Carte> chemin2 = new List<Carte>();

            for (int i = 0; i < cartesTriees.Count; i++)
            {
                if (i == indiceP1 || i == indiceP2 + 1)
                    continue;

                chemin2.Add(cartesTriees[i]);
            }

            // =====================================
            // CHEMIN 3
            // On saute P1C2 et P2C1
            // =====================================
            List<Carte> chemin3 = new List<Carte>();

            for (int i = 0; i < cartesTriees.Count; i++)
            {
                if (i == indiceP1 + 1 || i == indiceP2)
                    continue;

                chemin3.Add(cartesTriees[i]);
            }

            // =====================================
            // CHEMIN 4
            // On saute P1C2 et P2C2
            // =====================================
            List<Carte> chemin4 = new List<Carte>();

            for (int i = 0; i < cartesTriees.Count; i++)
            {
                if (i == indiceP1 + 1 || i == indiceP2 + 1)
                    continue;

                chemin4.Add(cartesTriees[i]);
            }

            // =====================================
            // CALCUL CHEMIN 1
            // =====================================
            Jeu jeu1 = new Jeu();
            bool bQuinte1 = false;

            Calcul_Moteur_Quinte_SansDoublon(chemin1, jeu1, ref bQuinte1, bouton);

            // =====================================
            // CALCUL CHEMIN 2
            // =====================================
            Jeu jeu2 = new Jeu();
            bool bQuinte2 = false;

            Calcul_Moteur_Quinte_SansDoublon(chemin2, jeu2, ref bQuinte2, bouton);

            // =====================================
            // CALCUL CHEMIN 3
            // =====================================
            Jeu jeu3 = new Jeu();
            bool bQuinte3 = false;

            Calcul_Moteur_Quinte_SansDoublon(chemin3, jeu3, ref bQuinte3, bouton);

            // =====================================
            // CALCUL CHEMIN 4
            // =====================================
            Jeu jeu4 = new Jeu();
            bool bQuinte4 = false;

            Calcul_Moteur_Quinte_SansDoublon(
                chemin4,
                jeu4,
                ref bQuinte4,
                bouton);


            // =====================================
            // CONSERVATION DU MEILLEUR RESULTAT
            // =====================================

            if (bQuinte1)
            {
                bQuinte = true;

                jeu.Force = jeu1.Force;
                jeu.Qualite = jeu1.Qualite;
                jeu.Participation = jeu1.Participation;
                jeu.EstQuinteFlush = jeu1.EstQuinteFlush;

                jeu.CartesQuinte.Clear();
                jeu.CartesQuinte.AddRange(jeu1.CartesQuinte);
            }

            if (bQuinte2 &&
                (!bQuinte ||
                 jeu2.Force > jeu.Force ||
                 (jeu2.Force == jeu.Force && jeu2.Qualite > jeu.Qualite)))
            {
                bQuinte = true;

                jeu.Force = jeu2.Force;
                jeu.Qualite = jeu2.Qualite;
                jeu.Participation = jeu2.Participation;
                jeu.EstQuinteFlush = jeu2.EstQuinteFlush;

                jeu.CartesQuinte.Clear();
                jeu.CartesQuinte.AddRange(jeu2.CartesQuinte);
            }

            if (bQuinte3 &&
                (!bQuinte ||
                 jeu3.Force > jeu.Force ||
                 (jeu3.Force == jeu.Force && jeu3.Qualite > jeu.Qualite)))
            {
                bQuinte = true;

                jeu.Force = jeu3.Force;
                jeu.Qualite = jeu3.Qualite;
                jeu.Participation = jeu3.Participation;
                jeu.EstQuinteFlush = jeu3.EstQuinteFlush;

                jeu.CartesQuinte.Clear();
                jeu.CartesQuinte.AddRange(jeu3.CartesQuinte);
            }

            if (bQuinte4 &&
                (!bQuinte ||
                 jeu4.Force > jeu.Force ||
                 (jeu4.Force == jeu.Force && jeu4.Qualite > jeu.Qualite)))
            {
                bQuinte = true;

                jeu.Force = jeu4.Force;
                jeu.Qualite = jeu4.Qualite;
                jeu.Participation = jeu4.Participation;
                jeu.EstQuinteFlush = jeu4.EstQuinteFlush;

                jeu.CartesQuinte.Clear();
                jeu.CartesQuinte.AddRange(jeu4.CartesQuinte);
            }

            // =====================================
            // CORRECTION DE LA PARTICIPATION
            // =====================================
            if (bQuinte)
            {
                foreach (Carte carteMain in jeu.CartesQuinte.Where(c => c.EstMain))
                {
                    if (cartes.Any(c => !c.EstMain &&
                                        c.Valeur == carteMain.Valeur))
                    {
                        jeu.Participation--;
                    }
                }

                if (jeu.Participation < 0)
                    jeu.Participation = 0;
            }
            // =====================================
            // CONSERVATION DU MEILLEUR RESULTAT
            // =====================================

            // ... les 4 if (bQuinte1)...(bQuinte4)

            // =====================================
            // CORRECTION DE LA PARTICIPATION
            // =====================================

            if (bQuinte)
            {
                foreach (Carte carteMain in jeu.CartesQuinte.Where(c => c.EstMain))
                {
                    if (cartes.Any(c => !c.EstMain &&
                                        c.Valeur == carteMain.Valeur))
                    {
                        jeu.Participation--;
                    }
                }

                if (jeu.Participation < 0)
                    jeu.Participation = 0;
            }
        }

        // =========================================================
        // 🔥 QUINTE RIVER (1 BRELAN)
        // 3 chemins
        // =========================================================
        private void Calcul_Moteur_Quinte_River_1Brelan(List<Carte> cartes, Jeu jeu, ref bool bQuinte, Button bouton)
        {
            // =====================================
            // RECHERCHE DU BRELAN
            // =====================================
            int indiceBrelan = -1;

            // =====================================
            // TRI
            // =====================================
            List<Carte> cartesTriees = cartes
                .OrderBy(c => c.Valeur)
                .ToList();


            for (int i = 0; i < cartesTriees.Count - 2; i++)
            {
                if (cartesTriees[i].Valeur == cartesTriees[i + 1].Valeur &&
                    cartesTriees[i].Valeur == cartesTriees[i + 2].Valeur)
                {
                    indiceBrelan = i;
                    break;
                }
            }

            // =====================================
            // RECHERCHE DU BRELAN
            // =====================================
            indiceBrelan = -1;

            for (int i = 0; i < cartesTriees.Count - 1; i++)
            {
                if (cartesTriees[i].Valeur == cartesTriees[i + 1].Valeur)
                {
                    indiceBrelan = i;
                    break;
                }
            }

            if (indiceBrelan == -1)
                return;

            // =====================================
            // CHEMIN 1
            // On saute la 1ère carte du brelan
            // =====================================
            List<Carte> chemin1 = new List<Carte>();

            for (int i = 0; i < cartesTriees.Count; i++)
            {
                if (i == indiceBrelan)
                    continue;

                chemin1.Add(cartesTriees[i]);
            }

            // =====================================
            // CHEMIN 2
            // On saute la 2ème carte du brelan
            // =====================================
            List<Carte> chemin2 = new List<Carte>();

            for (int i = 0; i < cartesTriees.Count; i++)
            {
                if (i == indiceBrelan + 1)
                    continue;

                chemin2.Add(cartesTriees[i]);
            }

            // =====================================
            // CHEMIN 3
            // On saute la 3ème carte du brelan
            // =====================================
            List<Carte> chemin3 = new List<Carte>();

            for (int i = 0; i < cartesTriees.Count; i++)
            {
                if (i == indiceBrelan + 2)
                    continue;

                chemin3.Add(cartesTriees[i]);
            }
            // =====================================
            // CALCUL CHEMIN 1
            // =====================================
            Jeu jeu1 = new Jeu();
            bool bQuinte1 = false;

            Calcul_Moteur_Quinte_SansDoublon(
                chemin1,
                jeu1,
                ref bQuinte1,
                bouton);

            // =====================================
            // CALCUL CHEMIN 2
            // =====================================
            Jeu jeu2 = new Jeu();
            bool bQuinte2 = false;

            Calcul_Moteur_Quinte_SansDoublon(
                chemin2,
                jeu2,
                ref bQuinte2,
                bouton);

            // =====================================
            // CALCUL CHEMIN 3
            // =====================================
            Jeu jeu3 = new Jeu();
            bool bQuinte3 = false;

            Calcul_Moteur_Quinte_SansDoublon(
                chemin3,
                jeu3,
                ref bQuinte3,
                bouton);



            // =====================================
            // CONSERVATION DU MEILLEUR RESULTAT
            // =====================================
            if (bQuinte1)
            {
                bQuinte = true;

                jeu.Force = jeu1.Force;
                jeu.Qualite = jeu1.Qualite;
                jeu.Participation = jeu1.Participation;
                jeu.EstQuinteFlush = jeu1.EstQuinteFlush;

                jeu.CartesQuinte.Clear();
                jeu.CartesQuinte.AddRange(jeu1.CartesQuinte);
            }

            if (bQuinte2 && (!bQuinte || jeu2.Qualite > jeu.Qualite))
            {
                bQuinte = true;

                jeu.Force = jeu2.Force;
                jeu.Qualite = jeu2.Qualite;
                jeu.Participation = jeu2.Participation;
                jeu.EstQuinteFlush = jeu2.EstQuinteFlush;

                jeu.CartesQuinte.Clear();
                jeu.CartesQuinte.AddRange(jeu2.CartesQuinte);
            }

            // =====================================
            // CORRECTION DE LA PARTICIPATION
            // =====================================
            if (bQuinte)
                CorrigerParticipationQuinteRiver(cartes, jeu);
        }

        // =========================================================
        // 🔥 QUINTE SANS DOUBLON
        // =========================================================
        private void Calcul_Moteur_Quinte_SansDoublon(List<Carte> cartes, Jeu jeu, ref bool bQuinte, Button bouton)
        {
            // RESET
            bQuinte = false;

            jeu.CartesQuinte.Clear();

            jeu.Force = 0;
            jeu.Qualite = 0;
            jeu.Participation = 0;

            jeu.EstQuinteFlush = false;

            // Trier les cartes
            var cartesTriees = cartes
                .OrderBy(c => c.Valeur)
                .ToList();

            // Gestion de l'As bas
            List<Carte> travail = cartesTriees.ToList();

            if (travail.Any(c => c.Valeur == 14))
            {
                Carte carteAs = travail.First(c => c.Valeur == 14);

                travail.Insert(0, new Carte
                {
                    Valeur = 1,
                    Couleur = carteAs.Couleur,
                    EstMain = carteAs.EstMain
                });
            }

            // =========================================================
            // RECHERCHE D'UNE QUINTE
            // =========================================================
            int suite = 1;

            for (int i = 1; i < travail.Count; i++)
            {
                // Même valeur (paire)
                if (travail[i].Valeur == travail[i - 1].Valeur)
                    continue;

                // Carte suivante
                if (travail[i].Valeur == travail[i - 1].Valeur + 1)
                {
                    suite++;

                    if (suite == 5)
                    {
                        bQuinte = true;

                        jeu.CartesQuinte.Clear();

                        for (int j = i - 4; j <= i; j++)
                            jeu.CartesQuinte.Add(travail[j]);

                        break;
                    }
                }
                else
                {
                    suite = 1;
                }
            }

            // =========================================================
            // AUCUNE QUINTE
            // =========================================================
            if (!bQuinte)
            {
                bouton.Background = Blanc;
                return;
            }

            // =========================================================
            // QUINTE OU QUINTE FLUSH
            // =========================================================

            jeu.EstQuinteFlush =
                jeu.CartesQuinte.All(c => c.Couleur == jeu.CartesQuinte[0].Couleur);

            // Force de la combinaison
            jeu.Force = jeu.EstQuinteFlush ? 8 : 4;

            // =========================================================
            // QUALITE
            // =========================================================

            jeu.Qualite = jeu.CartesQuinte.Max(c => c.Valeur);

            // Cas A2345
            if (jeu.Qualite == 14 &&
                jeu.CartesQuinte.Any(c => c.Valeur == 2))
            {
                jeu.Qualite = 5;
            }

            // =========================================================
            // PARTICIPATION
            // =========================================================

            jeu.Participation =
                jeu.CartesQuinte.Count(c => c.EstMain);

            // =========================================================
            // AFFICHAGE
            // =========================================================

            bouton.Background = Vert;
        }

        // =========================================================
        // 🔥 COULEUR
        // =========================================================
        private void Calcul_Moteur_Couleur(List<Carte> cartes, Jeu jeu, ref bool bCouleur, Button bouton)
        {
            // RESET
            bCouleur = false;

            // RECHERCHE D'UNE COULEUR
            var couleur = cartes
                .GroupBy(c => c.Couleur)
                .FirstOrDefault(g => g.Count() >= 5);

            if (couleur == null)
                return;

            // COULEUR TROUVEE
            bCouleur = true;

            jeu.Force = Math.Max(jeu.Force, 5);

            // Carte la plus haute de la couleur
            jeu.Qualite = couleur.Max(c => c.Valeur);

            // Nombre de cartes de ma main
            jeu.Participation =
                couleur.Count(c => c.EstMain);

            bouton.Background = Vert;
        }

        // =========================================================
        // 🔥 FULL
        // =========================================================
        private void Calcul_Moteur_Full(List<Carte> cartes, Jeu jeu, ref bool bFull, Button bouton)
        {
            // RESET
            bFull = false;

            // RECHERCHE
            var brelan = cartes
                .GroupBy(c => c.Valeur)
                .FirstOrDefault(g => g.Count() == 3);

            var paire = cartes
                .GroupBy(c => c.Valeur)
                .FirstOrDefault(g => g.Count() == 2);

            if (brelan == null || paire == null)
                return;

            // FULL TROUVE
            bFull = true;

            jeu.Force = Math.Max(jeu.Force, 6);

            // Valeur du brelan
            jeu.Qualite = brelan.Key;

            // Participation
            jeu.Participation =
                brelan.Count(c => c.EstMain) +
                paire.Count(c => c.EstMain);

            bouton.Background = Vert;
        }

        // =========================================================
        // 🔥 CARRE
        // =========================================================
        private void Calcul_Moteur_Carre(List<Carte> cartes, Jeu jeu, ref bool bCarre, Button bouton)
        {
            // RESET
            bCarre = false;

            // RECHERCHE DU CARRE
            var carre = cartes
                .GroupBy(c => c.Valeur)
                .FirstOrDefault(g => g.Count() == 4);

            if (carre == null)
                return;

            // CARRE TROUVE
            bCarre = true;

            jeu.Force = Math.Max(jeu.Force, 7);

            // Valeur du carré
            jeu.Qualite = carre.Key;

            // Nombre de cartes de ma main
            jeu.Participation =
                carre.Count(c => c.EstMain);

            bouton.Background = Vert;
        }

        // =========================================================
        // 🔥 CORRIGER PARTICIPATION QUINTE RIVER
        // =========================================================
        private void CorrigerParticipationQuinteRiver(List<Carte> cartes, Jeu jeu)
        {
            if (jeu.Participation == 0)
                return;

            foreach (Carte carteMain in jeu.CartesQuinte.Where(c => c.EstMain))
            {
                bool remplaçable = cartes.Any(c =>
                    !c.EstMain &&
                    c.Valeur == carteMain.Valeur);

                if (remplaçable)
                    jeu.Participation--;
            }

            if (jeu.Participation < 0)
                jeu.Participation = 0;
        }

        #endregion

        #region CALCUL PREFLOP
        // =========================================================
        // 🔥 CALCUL PREFLOP
        // =========================================================
        private void Calcul_PreFlop()
        {
            // =========================================================
            // 🔥 NOUVELLE MAIN
            // =========================================================

            // Remise à zéro de tous les indicateurs
            Initialiser_bPreFlop();
            Initialiser_bFlop();
            Initialiser_bTurn();
            Initialiser_bRiver();

            // Nouvelle liste de cartes
            TableCartes.Clear();

            TableCartes.Add(CreerCarte(TB_M1.Text, true));
            TableCartes.Add(CreerCarte(TB_M2.Text, true));

            // Nouveau résultat
            JeuPreFlop = new Jeu();

            if (TableCartes.Count < 2)
                return;

            Calcul_Moteur_Paire(TableCartes, JeuPreFlop, ref bPreFlop_Paire, Bt_PreFlop_Paire);
            Calcul_Moteur_Pareille(TableCartes, ref bPreFlop_Pareille, Bt_PreFlop_Pareille);
            Calcul_Moteur_Suite(TableCartes, ref bPreFlop_Suite, Bt_PreFlop_Suite);

            Debug_Jeu("PRE FLOP", JeuPreFlop);
        }

        private void Initialiser_bPreFlop()
        {
            bPreFlop_Paire = false;
            bPreFlop_Pareille = false;
            bPreFlop_Suite = false;

            // =========================================================
            // 🔥 RESET BOUTONS PREFLOP
            // =========================================================
            Bt_PreFlop_Paire.Background = Blanc;
            Bt_PreFlop_Suite.Background = Blanc;
            Bt_PreFlop_Pareille.Background = Blanc;
        }
        #endregion

        #region CALCUL FLOP

        // =========================================================
        // 🔥 CALCUL FLOP
        // =========================================================
        private void Calcul_Flop()
        {
            //Initialiser_bFlop();

            // =========================================================
            // 🔥 AJOUT DES CARTES DU FLOP
            // =========================================================
            if (TableCartes.Count == 2)
            {
                TableCartes.Add(CreerCarte(TB_F1.Text, false));

                TableCartes.Add(CreerCarte(TB_F2.Text, false));

                TableCartes.Add(CreerCarte(TB_F3.Text, false));
            }

            // =========================================================
            // 🔥 SECURITE
            // =========================================================
            if (TableCartes.Count != 5)
                return;

            JeuFlop = new Jeu();

            // =========================================================
            // 🔥 DU PLUS FORT AU PLUS FAIBLE
            // =========================================================

            Calcul_Moteur_Carre(TableCartes, JeuFlop, ref bFlop_Carre, Bt_Flop_Carre);

            if (bFlop_Carre)
                return;


            Calcul_Moteur_Full(TableCartes, JeuFlop, ref bFlop_Full, Bt_Flop_Full);

            if (bFlop_Full)
                return;

            // =========================================================
            // 🔥 QUINTE
            // Le moteur Quinte détecte aussi la Quinte Flush
            // =========================================================
            Calcul_Moteur_Quinte(TableCartes, JeuFlop, ref bFlop_Quinte, Bt_Flop_Quinte);

            if (bFlop_Quinte)
                return;

            Calcul_Moteur_Brelan(TableCartes, JeuFlop, ref bFlop_Brelan, Bt_Flop_Brelan);

            Calcul_Moteur_Couleur(TableCartes, JeuFlop, ref bFlop_Couleur, Bt_Flop_Couleur);

            if (bFlop_Brelan)
                return;

            Calcul_Moteur_DeuxPaires(TableCartes, JeuFlop, ref bFlop_2_Paires, Bt_Flop_2_Paires);

            if (bFlop_2_Paires)
                return;

            Calcul_Moteur_Paire(TableCartes, JeuFlop, ref bFlop_Paire, Bt_Flop_Paire);
        }

        // =========================================================
        // 🔥 INITIALISER FLOP
        // =========================================================
        private void Initialiser_bFlop()
        {
            bFlop_Paire = false;
            bFlop_2_Paires = false;
            bFlop_Brelan = false;
            bFlop_Quinte = false;
            bFlop_Couleur = false;
            bFlop_Full = false;
            bFlop_Carre = false;
            bFlop_QFlush = false;

            bTir_Flop_Quinte = false;
            bTir_Flop_Ventrale = false;
            bTir_Flop_Couleur = false;

            // =========================================================
            // 🔥 RESET BOUTONS FLOP
            // =========================================================
            Bt_Tir_Flop_Quinte.Content = "Q";

            Bt_Flop_Paire.Background = Blanc;
            Bt_Flop_2_Paires.Background = Blanc;
            Bt_Flop_Brelan.Background = Blanc;
            Bt_Flop_Quinte.Background = Blanc;
            Bt_Flop_Couleur.Background = Blanc;
            Bt_Flop_Full.Background = Blanc;
            Bt_Flop_Carre.Background = Blanc;
            Bt_Flop_Quinte_Flush.Background = Blanc;
        }


        #endregion

        #region CALCUL TURN

        // =========================================================
        // 🔥 CALCUL TURN
        // =========================================================
        private void Calcul_Turn()
        {
            Initialiser_bTurn();

            // =========================================================
            // 🔥 AJOUT DE LA CARTE DU TURN
            // =========================================================
            if (TableCartes.Count == 5)
            {
                TableCartes.Add(CreerCarte(TB_T.Text, false));
            }

            // =========================================================
            // 🔥 SECURITE
            // =========================================================
            if (TableCartes.Count != 6)
                return;

            JeuTurn = new Jeu();

            // =========================================================
            // 🔥 DU PLUS FORT AU PLUS FAIBLE
            // =========================================================

            Calcul_Moteur_Carre(TableCartes, JeuTurn, ref bTurn_Carre, Bt_Turn_Carre);

            if (bTurn_Carre)
            {
                Debug_Jeu("TURN", JeuTurn);

                return;
            }

            Calcul_Moteur_Full(TableCartes, JeuTurn, ref bTurn_Full, Bt_Turn_Full);

            if (bTurn_Full)
            {
                Debug_Jeu("TURN", JeuTurn);

                return;
            }

            Calcul_Moteur_Quinte(TableCartes, JeuTurn, ref bTurn_Quinte, Bt_Turn_Quinte);

            if (bTurn_Quinte)
            {
                Debug_Jeu("TURN", JeuTurn);

                return;
            }

            Calcul_Moteur_Couleur(TableCartes, JeuTurn, ref bTurn_Couleur, Bt_Turn_Couleur);

            if (bTurn_Couleur)
            {
                Debug_Jeu("TURN", JeuTurn);

                return;
            }

            Calcul_Moteur_Brelan(TableCartes, JeuTurn, ref bTurn_Brelan, Bt_Turn_Brelan);

            if (bTurn_Brelan)
            {
                Debug_Jeu("TURN", JeuTurn);

                return;
            }

            Calcul_Moteur_DeuxPaires(TableCartes, JeuTurn, ref bTurn_2_Paires, Bt_Turn_2_Paires);

            if (bTurn_2_Paires)
            {
                Debug_Jeu("TURN", JeuTurn);

                return;
            }

            Calcul_Moteur_Paire(TableCartes, JeuTurn, ref bTurn_Paire, Bt_Turn_Paire);

            Debug_Jeu("TURN", JeuTurn);
        }

        // =========================================================
        // 🔥 INITIALISER TURN
        // =========================================================
        private void Initialiser_bTurn()
        {
            bTurn_Paire = false;
            bTurn_2_Paires = false;
            bTurn_Brelan = false;
            bTurn_Quinte = false;
            bTurn_Couleur = false;
            bTurn_Full = false;
            bTurn_Carre = false;
            bTurn_QFlush = false;

            bTir_Turn_Quinte = false;
            bTir_Turn_Ventrale = false;
            bTir_Turn_Couleur = false;
            // =========================================================
            // 🔥 RESET BOUTONS TURN
            // =========================================================
            Bt_Tir_Turn_Quinte.Content = "Q";

            Bt_Turn_Paire.Background = Blanc;
            Bt_Turn_2_Paires.Background = Blanc;
            Bt_Turn_Brelan.Background = Blanc;
            Bt_Turn_Quinte.Background = Blanc;
            Bt_Turn_Couleur.Background = Blanc;
            Bt_Turn_Full.Background = Blanc;
            Bt_Turn_Carre.Background = Blanc;
            Bt_Turn_Quinte_Flush.Background = Blanc;

            Bt_Tir_Turn_Quinte.Background = Blanc;
            Bt_Tir_Turn_Couleur.Background = Blanc;
            Bt_Tir_Turn_Quinte_Flush.Background = Blanc;
        }

        #endregion

        #region CALCUL RIVER

        // =========================================================
        // 🔥 CALCUL RIVER
        // =========================================================
        private void Calcul_River()
        {
            //Initialiser_bRiver();

            // =========================================================
            // 🔥 AJOUT DE LA CARTE DU RIVER
            // =========================================================
            if (TableCartes.Count == 6)
            {
                TableCartes.Add(CreerCarte(TB_R.Text, false));
            }

            // =========================================================
            // 🔥 SECURITE
            // =========================================================
            if (TableCartes.Count != 7)
                return;

            JeuRiver = new Jeu();

            // =========================================================
            // 🔥 DU PLUS FORT AU PLUS FAIBLE
            // =========================================================

            Calcul_Moteur_Carre(TableCartes, JeuRiver, ref bRiver_Carre, Bt_River_Carre);

            if (bRiver_Carre)
            {
                Debug_Jeu("RIVER", JeuRiver);

                return;
            }

            Calcul_Moteur_Full(TableCartes, JeuRiver, ref bRiver_Full, Bt_River_Full);

            if (bRiver_Full)
            {
                Debug_Jeu("RIVER", JeuRiver);

                return;
            }
            Calcul_Moteur_Quinte(TableCartes, JeuRiver, ref bRiver_Quinte, Bt_River_Quinte);

            if (bRiver_Quinte)
            {
                Debug_Jeu("RIVER", JeuRiver);

                return;
            }

            Calcul_Moteur_Couleur(TableCartes, JeuRiver, ref bRiver_Couleur, Bt_River_Couleur);

            if (bRiver_Couleur)
            {
                Debug_Jeu("RIVER", JeuRiver);

                return;
            }

            Calcul_Moteur_Brelan(TableCartes, JeuRiver, ref bRiver_Brelan, Bt_River_Brelan);

            if (bRiver_Brelan)
            {
                Debug_Jeu("RIVER", JeuRiver);

                return;
            }

            Calcul_Moteur_DeuxPaires(TableCartes, JeuRiver, ref bRiver_2_Paires, Bt_River_2_Paires);

            if (bRiver_2_Paires)
            {
                Debug_Jeu("RIVER", JeuRiver);

                return;
            }

            Calcul_Moteur_Paire(TableCartes, JeuRiver, ref bRiver_Paire, Bt_River_Paire);

            Debug_Jeu("RIVER", JeuRiver);
        }

        // =========================================================
        // 🔥 INITIALISER RIVER
        // =========================================================
        private void Initialiser_bRiver()
        {
            bRiver_Paire = false;
            bRiver_2_Paires = false;
            bRiver_Brelan = false;
            bRiver_Quinte = false;
            bRiver_Couleur = false;
            bRiver_Full = false;
            bRiver_Carre = false;
            bRiver_QFlush = false;

            // =========================================================
            // 🔥 RESET BOUTONS RIVER
            // =========================================================
            Bt_River_Paire.Background = Blanc;
            Bt_River_2_Paires.Background = Blanc;
            Bt_River_Brelan.Background = Blanc;
            Bt_River_Quinte.Background = Blanc;
            Bt_River_Couleur.Background = Blanc;
            Bt_River_Full.Background = Blanc;
            Bt_River_Carre.Background = Blanc;
            Bt_River_Quinte_Flush.Background = Blanc;
        }
        #endregion

        #region TIRAGES MOTEUR
        // =========================================================
        // 🔥 TIRAGES
        // =========================================================
        private void Calcul_Tirages()
        {
            // =====================================================
            // 🔥 FLOP
            // =====================================================
            if (TableCartes.Count == 5)
            {
                // =====================================
                // RESET TIRAGE FLOP
                // =====================================
                TirageFlop.Quinte = false;
                TirageFlop.Ventrale = false;
                TirageFlop.Couleur = false;
                TirageFlop.QuinteFlush = false;
                TirageFlop.Participation = 0;
                TirageFlop.Max = false;
                TirageFlop.CarteHaute = 0;

                // =====================================
                // RESET DANGERS FLOP
                // =====================================
                TirageFlop.DangerCarre = false;
                TirageFlop.DangerFull = false;
                TirageFlop.DangerQuinte = false;
                TirageFlop.DangerCouleur = false;
                TirageFlop.DangerQuinteFlush = false;

                // =====================================
                // RESET AFFICHAGE BOUTONS
                // =====================================
                Bt_Tir_Flop_Quinte.Background = Blanc;
                Bt_Tir_Flop_Quinte.Content = "Q";

                Bt_Tir_Flop_Couleur.Background = Blanc;

                Bt_Tir_Flop_Quinte_Flush.Background = Blanc;

                // =====================================
                // 🔥 TIRAGE QUINTE FLUSH
                // =====================================
                Tirage_Moteur_Quinte_Flush(TableCartes, TirageFlop);

                if (TirageFlop.QuinteFlush)
                {
                    Bt_Tir_Flop_Quinte_Flush.Background = Vert;
                }

                // =====================================
                // 🔥 TIRAGE COULEUR
                // =====================================
                Tirage_Moteur_Couleur(
                    TableCartes,
                    TirageFlop);

                if (TirageFlop.Couleur)
                {
                    Bt_Tir_Flop_Couleur.Background = Vert;
                }

                // =====================================
                // 🔥 TIRAGE QUINTE
                // Seulement s'il n'y a pas
                // de Couleur ni de Quinte Flush
                // =====================================
                if (!TirageFlop.Couleur &&
                    !TirageFlop.QuinteFlush)
                {
                    Tirage_Moteur_Quinte(TableCartes, TirageFlop);

                    if (TirageFlop.Quinte)
                    {
                        Bt_Tir_Flop_Quinte.Background = Vert;

                        if (TirageFlop.Ventrale)
                            Bt_Tir_Flop_Quinte.Content = "QV";
                        else
                            Bt_Tir_Flop_Quinte.Content = "Q";
                    }
                }

                // =====================================
                // DANGER CARRÉ
                // =====================================
                Danger_Moteur_Carre(TableCartes, TirageFlop);

                // =====================================
                // DANGER FULL
                // =====================================
                Danger_Moteur_Full(TableCartes, TirageFlop);

                // =====================================
                // DANGER QUINTE FLUSH
                // =====================================
                Danger_Moteur_Quinte_Flush(TableCartes, TirageFlop);
            }
            // =====================================================
            // 🔥 TURN
            // =====================================================
            else if (TableCartes.Count == 6)
            {
                // =====================================
                // RESET TIRAGE TURN
                // =====================================
                TirageTurn.Quinte = false;
                TirageTurn.Ventrale = false;
                TirageTurn.Couleur = false;
                TirageTurn.QuinteFlush = false;
                TirageTurn.Participation = 0;
                TirageTurn.Max = false;
                TirageTurn.CarteHaute = 0;

                // =====================================
                // RESET DANGERS TURN
                // =====================================
                TirageTurn.DangerCarre = false;
                TirageTurn.DangerFull = false;
                TirageTurn.DangerQuinte = false;
                TirageTurn.DangerCouleur = false;
                TirageTurn.DangerQuinteFlush = false;

                // =====================================
                // RESET AFFICHAGE BOUTONS
                // =====================================
                Bt_Tir_Turn_Quinte.Background = Blanc;
                Bt_Tir_Turn_Quinte.Content = "Q";

                Bt_Tir_Turn_Couleur.Background = Blanc;

                Bt_Tir_Turn_Quinte_Flush.Background = Blanc;

                // =====================================
                // TIRAGE QUINTE FLUSH
                // Exception :
                // reste calculé même si Couleur
                // =====================================
                Tirage_Moteur_Quinte_Flush(TableCartes, TirageTurn);

                if (TirageTurn.QuinteFlush)
                {
                    Bt_Tir_Turn_Quinte_Flush.Background = Vert;
                }

                // =====================================
                // TIRAGE COULEUR
                // =====================================
                Tirage_Moteur_Couleur(TableCartes, TirageTurn);

                if (TirageTurn.Couleur)
                {
                    Bt_Tir_Turn_Couleur.Background = Vert;
                }

                // =====================================
                // TIRAGE QUINTE
                // Seulement s'il n'y a PAS de Couleur
                // =====================================
                if (!TirageTurn.Couleur)
                {
                    Tirage_Moteur_Quinte(TableCartes, TirageTurn);

                    if (TirageTurn.Quinte)
                    {
                        Bt_Tir_Turn_Quinte.Background = Vert;

                        if (TirageTurn.Ventrale)
                            Bt_Tir_Turn_Quinte.Content = "QV";
                        else
                            Bt_Tir_Turn_Quinte.Content = "Q";
                    }
                }

                // =====================================
                // DANGER CARRÉ
                // =====================================
                Danger_Moteur_Carre(TableCartes, TirageTurn);

                // =====================================
                // DANGER FULL
                // =====================================
                Danger_Moteur_Full(TableCartes, TirageTurn);

                // =====================================
                // DANGER QUINTE FLUSH
                // =====================================
                Danger_Moteur_Quinte_Flush(TableCartes, TirageTurn);
            }


            // =====================================================
            // 🔥 RIVER
            // =====================================================
            else if (TableCartes.Count == 7)
            {
                // =====================================
                // RESET TIRAGE RIVER
                // =====================================
                TirageRiver.Quinte = false;
                TirageRiver.Ventrale = false;
                TirageRiver.Couleur = false;
                TirageRiver.QuinteFlush = false;
                TirageRiver.Participation = 0;
                TirageRiver.Max = false;
                TirageRiver.CarteHaute = 0;

                // =====================================
                // RESET DANGERS RIVER
                // =====================================
                TirageRiver.DangerCarre = false;
                TirageRiver.DangerFull = false;
                TirageRiver.DangerQuinte = false;
                TirageRiver.DangerCouleur = false;
                TirageRiver.DangerQuinteFlush = false;

                // =====================================
                // TIRAGE QUINTE FLUSH
                // =====================================
                Tirage_Moteur_Quinte_Flush(TableCartes, TirageRiver);

                // =====================================
                // TIRAGE QUINTE
                // =====================================
                Tirage_Moteur_Quinte(TableCartes, TirageRiver);

                // =====================================
                // TIRAGE COULEUR
                // =====================================
                Tirage_Moteur_Couleur(TableCartes, TirageRiver);

                // =====================================
                // DANGER CARRÉ
                // =====================================
                Danger_Moteur_Carre(TableCartes, TirageRiver);

                // =====================================
                // DANGER FULL
                // =====================================
                Danger_Moteur_Full(TableCartes, TirageRiver);

                // =====================================
                // DANGER QUINTE FLUSH
                // =====================================
                Danger_Moteur_Quinte_Flush(TableCartes, TirageRiver);
            }
        }
        // =========================================================
        // 🔥 TIRAGE QUINTE
        // Flop + Turn + River
        // =========================================================
        private void Tirage_Moteur_Quinte(List<Carte> cartes, Tirage tirage)
        {
            // =====================================
            // RESET
            // =====================================
            tirage.Quinte = false;
            tirage.Ventrale = false;
            tirage.DangerQuinte = false;
            tirage.Participation = 0;
            tirage.Max = false;
            tirage.CarteHaute = 0;

            // =====================================
            // VALEURS UNIQUES
            // =====================================
            List<int> valeurs = cartes
                .Select(c => c.Valeur)
                .Distinct()
                .ToList();

            // =====================================
            // AS PEUT ÊTRE UTILISÉ COMME 1
            // =====================================
            if (valeurs.Contains(14))
                valeurs.Add(1);

            // =====================================
            // LES 10 SUITES POSSIBLES
            // =====================================
            int[][] suites =
            {
                new[] { 1, 2, 3, 4, 5 },
                new[] { 2, 3, 4, 5, 6 },
                new[] { 3, 4, 5, 6, 7 },
                new[] { 4, 5, 6, 7, 8 },
                new[] { 5, 6, 7, 8, 9 },
                new[] { 6, 7, 8, 9, 10 },
                new[] { 7, 8, 9, 10, 11 },
                new[] { 8, 9, 10, 11, 12 },
                new[] { 9, 10, 11, 12, 13 },
                new[] { 10, 11, 12, 13, 14 }
            };

            // =====================================================
            // 🔥 1. QUINTE DÉJÀ RÉALISÉE ?
            // =====================================================
            foreach (int[] suite in suites)
            {
                int presentes =
                    suite.Count(v => valeurs.Contains(v));

                if (presentes == 5)
                {
                    // Une vraie quinte existe déjà.
                    return;
                }
            }

            // =====================================================
            // 🔥 2. RECHERCHE PRIORITAIRE D'UN TIRAGE OUVERT
            // 4 CARTES CONSÉCUTIVES
            // =====================================================
            foreach (int[] suite in suites)
            {
                int presentes =
                    suite.Count(v => valeurs.Contains(v));

                if (presentes != 4)
                    continue;

                // =====================================
                // VALEURS PRÉSENTES
                // =====================================
                List<int> valeursPresentes =
                    suite
                        .Where(v => valeurs.Contains(v))
                        .OrderBy(v => v)
                        .ToList();

                // =====================================
                // 4 CARTES CONSÉCUTIVES
                // → TIRAGE QUINTE
                // → PAS VENTRALE
                // =====================================
                bool quatreConsecutives = true;

                for (int i = 1; i < valeursPresentes.Count; i++)
                {
                    if (valeursPresentes[i] !=
                        valeursPresentes[i - 1] + 1)
                    {
                        quatreConsecutives = false;
                        break;
                    }
                }

                if (!quatreConsecutives)
                    continue;

                // =====================================
                // CARTES DE LA SUITE
                // =====================================
                List<Carte> cartesTirage =
                    cartes
                        .Where(c =>
                            suite.Contains(c.Valeur) ||
                            (suite.Contains(1) &&
                             c.Valeur == 14))
                        .ToList();

                // =====================================
                // VALEURS BOARD
                // =====================================
                HashSet<int> valeursBoard =
                    cartes
                        .Where(c => !c.EstMain)
                        .Select(c => c.Valeur)
                        .ToHashSet();

                // =====================================
                // PARTICIPATION DE MA MAIN
                // =====================================
                List<Carte> cartesMain =
                    cartesTirage
                        .Where(c =>
                            c.EstMain &&
                            !valeursBoard.Contains(c.Valeur))
                        .GroupBy(c => c.Valeur)
                        .Select(g => g.First())
                        .ToList();

                int participation =
                    cartesMain.Count;

                // =====================================
                // JE PARTICIPE
                // =====================================
                if (participation > 0)
                {
                    tirage.Quinte = true;
                    tirage.Ventrale = false;
                    tirage.Participation = participation;

                    tirage.CarteHaute =
                        cartesMain
                            .Select(c =>
                                (suite.Contains(1) &&
                                 c.Valeur == 14)
                                    ? 1
                                    : c.Valeur)
                            .Max();

                    return;
                }

                // =====================================
                // JE NE PARTICIPE PAS
                // → DANGER QUINTE
                // =====================================
                if (cartes.Count >= 6)
                {
                    tirage.DangerQuinte = true;
                    tirage.Ventrale = false;
                    tirage.Participation = 0;

                    return;
                }
            }

            // =====================================================
            // 🔥 3. RECHERCHE D'UNE VENTRALE
            // Seulement après avoir écarté les tirages ouverts
            // =====================================================
            foreach (int[] suite in suites)
            {
                int presentes =
                    suite.Count(v => valeurs.Contains(v));

                if (presentes != 4)
                    continue;

                List<int> valeursPresentes =
                    suite
                        .Where(v => valeurs.Contains(v))
                        .OrderBy(v => v)
                        .ToList();

                // =====================================
                // SI 4 CARTES CONSÉCUTIVES
                // → déjà traité au-dessus
                // =====================================
                bool quatreConsecutives = true;

                for (int i = 1; i < valeursPresentes.Count; i++)
                {
                    if (valeursPresentes[i] !=
                        valeursPresentes[i - 1] + 1)
                    {
                        quatreConsecutives = false;
                        break;
                    }
                }

                if (quatreConsecutives)
                    continue;

                // =====================================
                // RECHERCHE DU TROU INTÉRIEUR
                // =====================================
                int positionManquante = -1;

                for (int i = 0; i < suite.Length; i++)
                {
                    if (!valeurs.Contains(suite[i]))
                    {
                        positionManquante = i;
                        break;
                    }
                }

                // =====================================
                // TROU INTÉRIEUR UNIQUEMENT
                // =====================================
                if (positionManquante <= 0 ||
                    positionManquante >= 4)
                {
                    continue;
                }

                // =====================================
                // CARTES DE LA SUITE
                // =====================================
                List<Carte> cartesTirage =
                    cartes
                        .Where(c =>
                            suite.Contains(c.Valeur) ||
                            (suite.Contains(1) &&
                             c.Valeur == 14))
                        .ToList();

                // =====================================
                // VALEURS BOARD
                // =====================================
                HashSet<int> valeursBoard =
                    cartes
                        .Where(c => !c.EstMain)
                        .Select(c => c.Valeur)
                        .ToHashSet();

                // =====================================
                // PARTICIPATION DE MA MAIN
                // =====================================
                List<Carte> cartesMain =
                    cartesTirage
                        .Where(c =>
                            c.EstMain &&
                            !valeursBoard.Contains(c.Valeur))
                        .GroupBy(c => c.Valeur)
                        .Select(g => g.First())
                        .ToList();

                int participation =
                    cartesMain.Count;

                // =====================================
                // JE PARTICIPE
                // → VENTRALE
                // =====================================
                if (participation > 0)
                {
                    tirage.Quinte = true;
                    tirage.Ventrale = true;
                    tirage.Participation = participation;

                    tirage.CarteHaute =
                        cartesMain
                            .Select(c =>
                                (suite.Contains(1) &&
                                 c.Valeur == 14)
                                    ? 1
                                    : c.Valeur)
                            .Max();

                    return;
                }

                // =====================================
                // JE NE PARTICIPE PAS
                // → DANGER VENTRALE
                // =====================================
                if (cartes.Count >= 6)
                {
                    tirage.DangerQuinte = true;
                    tirage.Ventrale = true;
                    tirage.Participation = 0;

                    return;
                }
            }
        }

        public bool Ventrale;
        // =========================================================
        // 🔥 TIRAGE COULEUR
        // Flop + Turn + River
        // =========================================================
        private void Tirage_Moteur_Couleur(List<Carte> cartes, Tirage tirage)
        {
            // =====================================
            // RESET
            // =====================================
            tirage.Couleur = false;
            tirage.DangerCouleur = false;

            // =====================================
            // RECHERCHE D'UNE COULEUR
            // 3 cartes ou plus
            // =====================================
            var couleur = cartes
                .GroupBy(c => c.Couleur)
                .Where(g => g.Count() >= 3)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => g.Count(c => c.EstMain))
                .FirstOrDefault();

            // =====================================
            // AUCUNE COULEUR
            // =====================================
            if (couleur == null)
                return;

            // =====================================
            // NOMBRE DE CARTES DE LA COULEUR
            // =====================================
            int nombreCartesCouleur = couleur.Count();

            // =====================================
            // COULEUR DEJA REALISEE
            // 5 cartes ou plus
            // =====================================
            if (nombreCartesCouleur >= 5)
            {
                tirage.Couleur = false;
                tirage.DangerCouleur = false;
                return;
            }

            // =====================================
            // CARTES DE LA COULEUR
            // =====================================
            List<Carte> cartesTirage =
                couleur.ToList();

            // =====================================
            // PARTICIPATION
            // =====================================
            int participation =
                cartesTirage.Count(c => c.EstMain);

            // =====================================
            // TIRAGE COULEUR
            // Il faut 4 cartes de la couleur
            // ET au moins 1 carte de ma main
            // =====================================
            if (nombreCartesCouleur == 4 &&
                participation > 0)
            {
                tirage.Couleur = true;
                tirage.DangerCouleur = false;

                // =====================================
                // CARTE HAUTE
                // =====================================
                int carteHaute = cartesTirage
                    .Where(c => c.EstMain)
                    .Max(c => c.Valeur);

                // =====================================
                // PRIORITÉ :
                // Participation puis CarteHaute
                // =====================================
                if (participation > tirage.Participation)
                {
                    tirage.Participation = participation;
                    tirage.CarteHaute = carteHaute;
                }
                else if (participation == tirage.Participation &&
                         carteHaute > tirage.CarteHaute)
                {
                    tirage.CarteHaute = carteHaute;
                }

                return;
            }

            // =====================================
            // JE NE PARTICIPE PAS
            // 3 cartes ou plus
            // → DANGER COULEUR
            // =====================================
            if (participation == 0 &&
                nombreCartesCouleur >= 3)
            {
                tirage.DangerCouleur = true;
            }
        }

        // =========================================================
        // 🔥 TIRAGE QUINTE FLUSH
        // Flop + Turn + River
        // 4 cartes sur 5
        // Je participe réellement
        // =========================================================
        private void Tirage_Moteur_Quinte_Flush(
            List<Carte> cartes,
            Tirage tirage)
        {
            // =====================================
            // RESET
            // =====================================
            tirage.QuinteFlush = false;

            // =====================================
            // MINIMUM 5 CARTES
            // Flop = 5 cartes
            // =====================================
            if (cartes.Count < 5)
                return;

            // =====================================
            // LES 10 SUITES POSSIBLES
            // =====================================
            int[][] suites =
            {
                new[] { 1, 2, 3, 4, 5 },
                new[] { 2, 3, 4, 5, 6 },
                new[] { 3, 4, 5, 6, 7 },
                new[] { 4, 5, 6, 7, 8 },
                new[] { 5, 6, 7, 8, 9 },
                new[] { 6, 7, 8, 9, 10 },
                new[] { 7, 8, 9, 10, 11 },
                new[] { 8, 9, 10, 11, 12 },
                new[] { 9, 10, 11, 12, 13 },
                new[] { 10, 11, 12, 13, 14 }
            };

            // =====================================
            // RECHERCHE COULEUR PAR COULEUR
            // =====================================
            foreach (var groupe in cartes.GroupBy(c => c.Couleur))
            {
                // =====================================
                // VALEURS UNIQUES DE CETTE COULEUR
                // =====================================
                List<int> valeursCouleur = groupe
                    .Select(c => c.Valeur)
                    .Distinct()
                    .ToList();

                // =====================================
                // AS = 1
                // =====================================
                if (valeursCouleur.Contains(14))
                    valeursCouleur.Add(1);

                // =====================================
                // RECHERCHE D'UNE SUITE À 4 CARTES
                // =====================================
                foreach (int[] suite in suites)
                {
                    int presentes =
                        suite.Count(v =>
                            valeursCouleur.Contains(v));

                    if (presentes != 4)
                        continue;

                    // =====================================
                    // CARTES DE LA SUITE / COULEUR
                    // =====================================
                    List<Carte> cartesQFlush = groupe
                        .Where(c =>
                            suite.Contains(c.Valeur) ||
                            (suite.Contains(1) &&
                             c.Valeur == 14))
                        .ToList();

                    // =====================================
                    // SECURITE
                    // =====================================
                    if (cartesQFlush.Count != 4)
                        continue;

                    // =====================================
                    // PARTICIPATION REELLE DE MA MAIN
                    // =====================================
                    bool jeParticipe =
                        cartesQFlush.Any(c => c.EstMain);

                    // =====================================
                    // JE NE PARTICIPE PAS
                    // → LE MOTEUR DANGER S'EN OCCUPE
                    // =====================================
                    if (!jeParticipe)
                        continue;

                    // =====================================
                    // 🔥 TIRAGE QUINTE FLUSH
                    // =====================================
                    tirage.QuinteFlush = true;

                    // =====================================
                    // CARTE HAUTE DU TIRAGE
                    // =====================================
                    List<Carte> cartesMain = cartesQFlush
                        .Where(c => c.EstMain)
                        .ToList();

                    if (cartesMain.Count > 0)
                    {
                        tirage.CarteHaute = cartesMain
                            .Select(c =>
                                (suite.Contains(1) &&
                                 c.Valeur == 14)
                                    ? 1
                                    : c.Valeur)
                            .Max();
                    }

                    return;
                }
            }
        }


        private Tirage TirageFlop = new Tirage();
        private Tirage TirageTurn = new Tirage();
        private Tirage TirageRiver = new Tirage();

        #endregion

        #region DANGERS

        // =========================================================
        // 🔥 DANGER QUINTE FLUSH
        // Turn + River
        // =========================================================
        private void Danger_Moteur_Quinte_Flush(List<Carte> cartes, Tirage danger)
        {

            // =====================================
            // RESET
            // =====================================
            danger.DangerQuinteFlush = false;

            if (cartes.Count < 6)
            {
                return;
            }

            // =====================================
            // LES 10 SUITES POSSIBLES
            // =====================================
            int[][] suites =
            {
                new[] { 1, 2, 3, 4, 5 },
                new[] { 2, 3, 4, 5, 6 },
                new[] { 3, 4, 5, 6, 7 },
                new[] { 4, 5, 6, 7, 8 },
                new[] { 5, 6, 7, 8, 9 },
                new[] { 6, 7, 8, 9, 10 },
                new[] { 7, 8, 9, 10, 11 },
                new[] { 8, 9, 10, 11, 12 },
                new[] { 9, 10, 11, 12, 13 },
                new[] { 10, 11, 12, 13, 14 }
            };

            foreach (var groupe in cartes.GroupBy(c => c.Couleur))
            {

                List<int> valeursCouleur = groupe
                    .Select(c => c.Valeur)
                    .Distinct()
                    .ToList();

                if (valeursCouleur.Contains(14))
                    valeursCouleur.Add(1);

                foreach (int[] suite in suites)
                {
                    int presentes =
                        suite.Count(v => valeursCouleur.Contains(v));

                    if (presentes == 4)
                    {

                        List<Carte> cartesQFlush = groupe
                            .Where(c =>
                                suite.Contains(c.Valeur) ||
                                (suite.Contains(1) && c.Valeur == 14))
                            .ToList();


                        if (cartesQFlush.Count == 4)
                        {
                            bool jeParticipe =
                                cartesQFlush.Any(c => c.EstMain);

                            //Debug("JE PARTICIPE = " + jeParticipe);

                            if (!jeParticipe)
                            {
                                //Debug("🔥 DANGER QFLUSH TROUVE");

                                danger.DangerQuinteFlush = true;
                                return;
                            }
                        }
                    }
                }
            }
        }

        // =========================================================
        // 🔥 DANGER CARRÉ
        // 3 cartes de même valeur sur le board
        // → il manque la 4e carte pour faire Carré
        // =========================================================
        private void Danger_Moteur_Carre(List<Carte> cartes, Tirage danger)
        {
            // =====================================
            // RESET
            // =====================================
            danger.DangerCarre = false;

            // =====================================
            // FLOP / TURN / RIVER
            // Il faut au moins 3 cartes sur le board
            // =====================================
            if (cartes.Count < 5)
                return;

            // =====================================
            // RECHERCHE DE 3 CARTES IDENTIQUES
            // SUR LE BOARD UNIQUEMENT
            // =====================================
            bool dangerCarre = cartes
                .Where(c => !c.EstMain)
                .GroupBy(c => c.Valeur)
                .Any(g => g.Count() == 3);

            // =====================================
            // DANGER CARRÉ
            // =====================================
            if (dangerCarre)
            {
                danger.DangerCarre = true;
            }
        }

        // =========================================================
        // 🔥 DANGER FULL
        // Turn + River
        // 2 paires sur le board
        // → une carte supplémentaire peut faire un Full
        // =========================================================
        private void Danger_Moteur_Full(List<Carte> cartes, Tirage danger)
        {
            // =====================================
            // RESET
            // =====================================
            danger.DangerFull = false;

            // =====================================
            // UNIQUEMENT TURN / RIVER
            // =====================================
            if (cartes.Count < 6)
                return;

            // =====================================
            // BOARD UNIQUEMENT
            // =====================================
            List<Carte> board = cartes
                .Where(c => !c.EstMain)
                .ToList();

            // =====================================
            // RECHERCHE DES PAIRES SUR LE BOARD
            // =====================================
            List<IGrouping<int, Carte>> paires = board
                .GroupBy(c => c.Valeur)
                .Where(g => g.Count() >= 2)
                .ToList();

            // =====================================
            // IL FAUT 2 VALEURS DIFFERENTES
            // EN DOUBLE SUR LE BOARD
            // =====================================
            if (paires.Count < 2)
                return;

            // =====================================
            // DANGER FULL
            // =====================================
            danger.DangerFull = true;
        }

        // =========================================================
        // 🔥 AFFICHER LE DANGER
        // Rouge + texte blanc + pictogramme
        // =========================================================
        private void Afficher_Danger(Tirage tirage)
        {
            // =====================================
            // DANGER CARRÉ
            // =====================================
            if (tirage.DangerCarre)
            {
                Bt_Danger.Content = "⚠Carré";
                FlashDanger();
            }

            // =====================================
            // DANGER FULL
            // =====================================
            else if (tirage.DangerFull)
            {
                Bt_Danger.Content = "⚠Full";
                FlashDanger();
            }

            // =====================================
            // DANGER QUINTE FLUSH
            // =====================================
            else if (tirage.DangerQuinteFlush)
            {
                Bt_Danger.Content = "⚠Quinte Flush";
                FlashDanger();
            }

            // =====================================
            // DANGER COULEUR + QUINTE
            // =====================================
            else if (tirage.DangerCouleur &&
                     tirage.DangerQuinte)
            {
                if (tirage.Ventrale)
                    Bt_Danger.Content = "⚠Couleur Quinte Ventrale";
                else
                    Bt_Danger.Content = "⚠Couleur Quinte";

                FlashDanger();
            }

            // =====================================
            // DANGER COULEUR
            // =====================================
            else if (tirage.DangerCouleur)
            {
                Bt_Danger.Content = "⚠Couleur";
                FlashDanger();
            }

            // =====================================
            // DANGER QUINTE
            // =====================================
            else if (tirage.DangerQuinte)
            {
                if (tirage.Ventrale)
                    Bt_Danger.Content = "⚠Quinte Ventrale";
                else
                    Bt_Danger.Content = "⚠Quinte";

                FlashDanger();
            }

            // =====================================
            // AUCUN DANGER
            // =====================================
            else
            {
                StopFlashDanger();
                Bt_Danger.Content = "";
            }
        }
        #endregion

        #region CHARGER EXTRAIRE LIRE SAUVER FERMER
        // =========================================================
        // 🔥 MAIN 1
        // =========================================================
        private void Charger_Main1_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Main_1.json");
            if (!File.Exists(fichier))
                return;
            string json = File.ReadAllText(fichier);
            Main1Detect = JsonConvert.DeserializeObject<S_CarteDetect[]>(json);
        }
        private bool Extraire_Main1(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;

            bool bMain1 = false;

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // FIN DU BLOC
                // =====================================

                if (bMain1 &&
                   (ligne == "COMMENT : Main_1_Presente" ||
                    ligne == "COMMENT : Main_2"))
                {
                    break;
                }

                // =====================================
                // COMMENT : Main_1
                // =====================================

                if (ligne == "COMMENT : Main_1")
                {
                    bMain1 = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================

                if (!bMain1)
                    continue;

                // =====================================
                // COMMENT : Carte
                // =====================================

                if (ligne.StartsWith("COMMENT :"))
                {
                    string nom = ligne.Replace("COMMENT :", "").Trim();

                    if (nom == "Main_1")
                        continue;

                    if (nom.Length < 2)
                        continue;

                    // =====================================
                    // Ligne suivante
                    // =====================================

                    if (i + 1 >= lignes.Length)
                        continue;

                    string l2 = lignes[i + 1].Trim();

                    if (!l2.StartsWith("IF PIXEL COLOR EQUALS"))
                        continue;

                    // =====================================
                    // Découpage
                    // =====================================

                    string[] t = l2.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =====================================
                    // Extraction
                    // =====================================
                    Main1Detect[index].Nom = nom;
                    Main1Detect[index].X = int.Parse(t[1]);
                    Main1Detect[index].Y = int.Parse(t[2]);
                    Main1Detect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 52)
                return false;

            return true;
        }
        private void Lire_Main_1()
        {
            CarteCourante = "";

            foreach (var carte in Main1Detect)
            {
                // =====================================
                // Sécurité
                // =====================================

                if (string.IsNullOrEmpty(carte.Nom))
                    continue;

                // =====================================
                // Lecture pixel
                // =====================================

                System.Drawing.Color pixel =
                    Bitmap_Main.GetPixel(
                        carte.X,
                        carte.Y);

                int couleurLue =
                    (pixel.R << 16) |
                    (pixel.G << 8) |
                     pixel.B;

                // =====================================
                // Correspondance EXACTE
                // =====================================

                if (couleurLue == carte.Couleur)
                {
                    CarteCourante =
                        carte.Nom;

                    AfficherCarte_Test(
                        TB_M1,
                        Bt_Main_1,
                        0);

                    return;
                }
            }
        }
        private void Sauver_Main1()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Main_1.json");
            Directory.CreateDirectory(DOSSIER_JSON);
            string json = JsonConvert.SerializeObject(Main1Detect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 MAIN 2
        // =========================================================
        private void Charger_Main2_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Main_2.json");
            if (!File.Exists(fichier))
                return;
            string json = File.ReadAllText(fichier);
            Main2Detect = JsonConvert.DeserializeObject<S_CarteDetect[]>(json);
        }
        private bool Extraire_Main2(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;

            bool bMain2 = false;

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // FIN DU BLOC
                // =====================================

                if (bMain2 &&
                   (ligne == "COMMENT : Main_2_Presente" ||
                    ligne == "COMMENT : Flop_1"))
                {
                    break;
                }

                // =====================================
                // COMMENT : Main_2
                // =====================================

                if (ligne == "COMMENT : Main_2")
                {
                    bMain2 = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================

                if (!bMain2)
                    continue;

                // =====================================
                // COMMENT : Carte
                // =====================================

                if (ligne.StartsWith("COMMENT :"))
                {
                    string nom = ligne.Replace("COMMENT :", "").Trim();

                    if (nom == "Main_2")
                        continue;

                    if (nom.Length < 2)
                        continue;

                    // =====================================
                    // Ligne suivante
                    // =====================================

                    if (i + 1 >= lignes.Length)
                        continue;

                    string l2 = lignes[i + 1].Trim();

                    if (!l2.StartsWith("IF PIXEL COLOR EQUALS"))
                        continue;

                    // =====================================
                    // Découpage
                    // =====================================

                    string[] t = l2.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =====================================
                    // Extraction
                    // =====================================
                    Main2Detect[index].Nom = nom;
                    Main2Detect[index].X = int.Parse(t[1]);
                    Main2Detect[index].Y = int.Parse(t[2]);
                    Main2Detect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 52)
                return false;

            return true;
        }
        private void Lire_Main_2()
        {
            CarteCourante = "";

            foreach (var carte in Main2Detect)
            {
                // =====================================
                // Sécurité
                // =====================================

                if (string.IsNullOrEmpty(carte.Nom))
                    continue;

                // =====================================
                // Lecture pixel
                // =====================================

                System.Drawing.Color pixel = Bitmap_Main.GetPixel(carte.X, carte.Y);

                int couleurLue = (pixel.R << 16) | (pixel.G << 8) | pixel.B;

                // =====================================
                // Correspondance EXACTE
                // =====================================

                if (couleurLue == carte.Couleur)
                {
                    CarteCourante = carte.Nom;

                    AfficherCarte_Test(TB_M2, Bt_Main_2, 1);

                    return;
                }
            }
        }
        private void Sauver_Main2()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Main_2.json");
            Directory.CreateDirectory(DOSSIER_JSON); ;
            string json = JsonConvert.SerializeObject(Main2Detect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 FLOP 1
        // =========================================================
        private void Charger_Flop1_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Flop_1.json");
            if (!File.Exists(fichier))
                return;
            string json = File.ReadAllText(fichier);
            Flop1Detect = JsonConvert.DeserializeObject<S_CarteDetect[]>(json);
        }
        private bool Extraire_Flop1(string txt)
        {
            string[] lignes =
                txt.Split('\n');

            int index = 0;

            bool bFlop1 = false;

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne =
                    lignes[i].Trim();

                // =====================================
                // COMMENT : Flop_1
                // =====================================

                if (ligne == "COMMENT : Flop_1")
                {
                    bFlop1 = true;
                    continue;
                }

                // =====================================
                // FIN DU BLOC
                // =====================================

                if (bFlop1 &&
                    ligne == "COMMENT : Flop_2")
                {
                    break;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================

                if (!bFlop1)
                    continue;

                // =====================================
                // COMMENT : Carte
                // =====================================

                if (ligne.StartsWith("COMMENT :"))
                {
                    string nom = ligne.Replace("COMMENT :", "").Trim();

                    if (nom == "Flop_1")
                        continue;

                    if (nom.Length < 2)
                        continue;

                    // =====================================
                    // Ligne suivante
                    // =====================================

                    if (i + 1 >= lignes.Length)
                        continue;

                    string l2 = lignes[i + 1].Trim();

                    if (!l2.StartsWith("IF PIXEL COLOR EQUALS"))
                        continue;

                    // =====================================
                    // Découpage
                    // =====================================

                    string[] t = l2.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =====================================
                    // Extraction
                    // =====================================
                    Flop1Detect[index].Nom = nom;
                    Flop1Detect[index].X = int.Parse(t[1]);
                    Flop1Detect[index].Y = int.Parse(t[2]);
                    Flop1Detect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 52)
                return false;

            return true;
        }
        private void Lire_Flop_1()
        {
            CarteCourante = "";

            foreach (var carte in Flop1Detect)
            {
                // =====================================
                // Sécurité
                // =====================================

                if (string.IsNullOrEmpty(carte.Nom))
                    continue;

                // =====================================
                // Lecture pixel
                // =====================================

                System.Drawing.Color pixel =
                    Bitmap_Main.GetPixel(
                        carte.X,
                        carte.Y);

                int couleurLue =
                    (pixel.R << 16) |
                    (pixel.G << 8) |
                     pixel.B;

                // =====================================
                // Correspondance EXACTE
                // =====================================

                if (couleurLue == carte.Couleur)
                {
                    CarteCourante =
                        carte.Nom;

                    AfficherCarte_Test(
                        TB_F1,
                        Bt_Flop_1,
                        2);

                    return;
                }
            }
        }
        private void Sauver_Flop1()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Flop_1.json");
            Directory.CreateDirectory(DOSSIER_JSON);
            string json = JsonConvert.SerializeObject(Flop1Detect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 FLOP 2 
        // =========================================================
        private void Charger_Flop2_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Flop_2.json");
            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);
            Flop2Detect = JsonConvert.DeserializeObject<S_CarteDetect[]>(json);
        }
        private bool Extraire_Flop2(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;

            bool bFlop2 = false;

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // COMMENT : Flop_2
                // =====================================

                if (ligne == "COMMENT : Flop_2")
                {
                    bFlop2 = true;
                    continue;
                }

                // =====================================
                // FIN DU BLOC
                // =====================================

                if (bFlop2 && ligne == "COMMENT : Flop_3")
                {
                    break;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================

                if (!bFlop2)
                    continue;

                // =====================================
                // COMMENT : Carte
                // =====================================

                if (ligne.StartsWith("COMMENT :"))
                {
                    string nom = ligne.Replace("COMMENT :", "").Trim();

                    if (nom == "Flop_2")
                        continue;

                    if (nom.Length < 2)
                        continue;

                    // =====================================
                    // Ligne suivante
                    // =====================================

                    if (i + 1 >= lignes.Length)
                        continue;

                    string l2 =
                        lignes[i + 1].Trim();

                    if (!l2.StartsWith(
                        "IF PIXEL COLOR EQUALS"))
                        continue;

                    // =====================================
                    // Découpage
                    // =====================================

                    string[] t =
                        l2.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =====================================
                    // Extraction
                    // =====================================

                    Flop2Detect[index].Nom = nom;
                    Flop2Detect[index].X = int.Parse(t[1]);
                    Flop2Detect[index].Y = int.Parse(t[2]);
                    Flop2Detect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 52)
                return false;

            return true;
        }
        private void Lire_Flop_2()
        {
            CarteCourante = "";

            foreach (var carte in Flop2Detect)
            {
                // =====================================
                // Sécurité
                // =====================================

                if (string.IsNullOrEmpty(carte.Nom))
                    continue;

                // =====================================
                // Lecture pixel
                // =====================================

                System.Drawing.Color pixel =
                    Bitmap_Main.GetPixel(
                        carte.X,
                        carte.Y);

                int couleurLue =
                    (pixel.R << 16) |
                    (pixel.G << 8) |
                     pixel.B;

                // =====================================
                // Correspondance EXACTE
                // =====================================

                if (couleurLue == carte.Couleur)
                {
                    CarteCourante =
                        carte.Nom;

                    AfficherCarte_Test(
                        TB_F2,
                        Bt_Flop_2,
                        3);

                    return;
                }
            }
        }
        private void Sauver_Flop2()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Flop_2.json");
            string json = JsonConvert.SerializeObject(Flop2Detect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 FLOP 3
        // =========================================================
        private void Charger_Flop3_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Flop_3.json");
            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);
            Flop3Detect = JsonConvert.DeserializeObject<S_CarteDetect[]>(json);
        }
        private bool Extraire_Flop3(string txt)
        {
            string[] lignes =
                txt.Split('\n');

            int index = 0;

            bool bFlop3 = false;

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne =
                    lignes[i].Trim();

                // =====================================
                // COMMENT : Flop_3
                // =====================================

                if (ligne == "COMMENT : Flop_3")
                {
                    bFlop3 = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================

                if (!bFlop3)
                    continue;

                // =====================================
                // COMMENT : Carte
                // =====================================

                if (ligne.StartsWith("COMMENT :"))
                {
                    string nom = ligne.Replace("COMMENT :", "").Trim();

                    if (nom == "Flop_3")
                        continue;

                    if (nom.Length < 2)
                        continue;

                    // =====================================
                    // Ligne suivante
                    // =====================================

                    if (i + 1 >= lignes.Length)
                        continue;

                    string l2 = lignes[i + 1].Trim();

                    if (!l2.StartsWith("IF PIXEL COLOR EQUALS"))
                        continue;

                    // =====================================
                    // Découpage
                    // =====================================

                    string[] t = l2.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =====================================
                    // Extraction
                    // =====================================

                    Flop3Detect[index].Nom = nom;

                    Flop3Detect[index].X = int.Parse(t[1]);

                    Flop3Detect[index].Y = int.Parse(t[2]);

                    Flop3Detect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 52)
                return false;

            return true;
        }
        private void Lire_Flop_3()
        {
            CarteCourante = "";

            foreach (var carte in Flop3Detect)
            {
                // =====================================
                // Sécurité
                // =====================================

                if (string.IsNullOrEmpty(carte.Nom))
                    continue;

                // =====================================
                // Lecture pixel
                // =====================================

                System.Drawing.Color pixel = Bitmap_Main.GetPixel(carte.X, carte.Y);

                int couleurLue = (pixel.R << 16) | (pixel.G << 8) | pixel.B;

                // =====================================
                // Correspondance EXACTE
                // =====================================

                if (couleurLue == carte.Couleur)
                {
                    CarteCourante = carte.Nom;

                    AfficherCarte_Test(TB_F3, Bt_Flop_3, 4);

                    return;
                }
            }
        }
        private void Sauver_Flop3()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Flop_3.json");
            string json = JsonConvert.SerializeObject(Flop3Detect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 TURN
        // =========================================================
        private void Charger_Turn_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Turn.json");
            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);
            TurnDetect = JsonConvert.DeserializeObject<S_CarteDetect[]>(json);
        }
        private bool Extraire_Turn(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;

            bool bTurn = false;

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // COMMENT : Turn
                // =====================================

                if (ligne == "COMMENT : Turn")
                {
                    bTurn = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================

                if (!bTurn)
                    continue;

                // =====================================
                // COMMENT : Carte
                // =====================================

                if (ligne.StartsWith("COMMENT :"))
                {
                    string nom = ligne.Replace("COMMENT :", "").Trim();

                    if (nom == "Turn")
                        continue;

                    if (nom.Length < 2)
                        continue;

                    // =====================================
                    // Ligne suivante
                    // =====================================
                    if (i + 1 >= lignes.Length)
                        continue;

                    string l2 = lignes[i + 1].Trim();

                    if (!l2.StartsWith("IF PIXEL COLOR EQUALS"))
                        continue;
                    // =====================================
                    // Découpage
                    // =====================================

                    string[] t = l2.Split(':');

                    if (t.Length < 5)
                        continue;
                    // =====================================
                    // Extraction
                    // =====================================

                    TurnDetect[index].Nom = nom;

                    TurnDetect[index].X = int.Parse(t[1]);

                    TurnDetect[index].Y = int.Parse(t[2]);

                    TurnDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }
            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 52)
                return false;

            return true;
        }
        private void Lire_Turn()
        {
            CarteCourante = "";

            foreach (var carte in TurnDetect)
            {
                // =====================================
                // Sécurité
                // =====================================

                if (string.IsNullOrEmpty(carte.Nom))
                    continue;

                // =====================================
                // Lecture pixel
                // =====================================

                System.Drawing.Color pixel = Bitmap_Main.GetPixel(carte.X, carte.Y);

                int couleurLue = (pixel.R << 16) | (pixel.G << 8) | pixel.B;

                // =====================================
                // Correspondance EXACTE
                // =====================================

                if (couleurLue == carte.Couleur)
                {
                    CarteCourante = carte.Nom;
                    AfficherCarte_Test(TB_T, Bt_Turn, 5);

                    return;
                }
            }
        }
        private void Sauver_Turn()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Turn.json");
            string json = JsonConvert.SerializeObject(TurnDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 TURN_HB
        // =========================================================
        private void Charger_Turn_HB_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Turn_HB.json");
            if (!File.Exists(fichier))
                return;
            string json = File.ReadAllText(fichier);
            Turn_HBDetect = JsonConvert.DeserializeObject<S_CarteDetect[]>(json);
        }
        private bool Extraire_Turn_HB(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;

            bool bTurn_HB = false;

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // COMMENT : Turn_HB
                // =====================================

                if (ligne == "COMMENT : Turn_HB")
                {
                    bTurn_HB = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================

                if (!bTurn_HB)
                    continue;

                // =====================================
                // COMMENT : Carte
                // =====================================

                if (ligne.StartsWith("COMMENT :"))
                {
                    string nom = ligne.Replace("COMMENT :", "").Trim();

                    if (nom == "Turn_HB")
                        continue;

                    if (nom.Length < 2)
                        continue;

                    // =====================================
                    // Ligne suivante
                    // =====================================

                    if (i + 1 >= lignes.Length)
                        continue;

                    string l2 = lignes[i + 1].Trim();

                    if (!l2.StartsWith("IF PIXEL COLOR EQUALS"))
                        continue;

                    // =====================================
                    // Découpage
                    // =====================================

                    string[] t = l2.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =====================================
                    // Extraction
                    // =====================================

                    Turn_HBDetect[index].Nom = nom;

                    Turn_HBDetect[index].X = int.Parse(t[1]);

                    Turn_HBDetect[index].Y = int.Parse(t[2]);

                    Turn_HBDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 104)
                return false;

            return true;
        }
        private void Lire_Turn_HB()
        {
            CarteCourante = "";

            foreach (var carte in Turn_HBDetect)
            {
                // =====================================
                // Sécurité
                // =====================================

                if (string.IsNullOrEmpty(carte.Nom))
                    continue;

                // =====================================
                // Lecture pixel
                // =====================================

                System.Drawing.Color pixel = Bitmap_Main.GetPixel(carte.X, carte.Y);

                int couleurLue = (pixel.R << 16) | (pixel.G << 8) | pixel.B;

                // =====================================
                // Correspondance EXACTE
                // =====================================

                if (couleurLue == carte.Couleur)
                {
                    CarteCourante = carte.Nom;
                    AfficherCarte_Test(TB_T, Bt_Turn, 5);

                    return;
                }
            }
        }
        private void Sauver_Turn_HB()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Turn_HB.json");
            string json = JsonConvert.SerializeObject(Turn_HBDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 RIVER
        // =========================================================
        private void Charger_River_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "River.json");
            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);
            RiverDetect = JsonConvert.DeserializeObject<S_CarteDetect[]>(json);
        }
        private bool Extraire_River(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;

            bool bRiver = false;

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // COMMENT : River
                // =====================================

                if (ligne == "COMMENT : River")
                {
                    bRiver = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================

                if (!bRiver)
                    continue;

                // =====================================
                // COMMENT : Carte
                // =====================================

                if (ligne.StartsWith("COMMENT :"))
                {
                    string nom = ligne.Replace("COMMENT :", "").Trim();

                    if (nom == "River")
                        continue;

                    if (nom.Length < 2)
                        continue;

                    // =====================================
                    // Ligne suivante
                    // =====================================
                    if (i + 1 >= lignes.Length)
                        continue;

                    string l2 = lignes[i + 1].Trim();

                    if (!l2.StartsWith("IF PIXEL COLOR EQUALS"))
                        continue;
                    // =====================================
                    // Découpage
                    // =====================================

                    string[] t = l2.Split(':');

                    if (t.Length < 5)
                        continue;
                    // =====================================
                    // Extraction
                    // =====================================

                    RiverDetect[index].Nom = nom;

                    RiverDetect[index].X = int.Parse(t[1]);

                    RiverDetect[index].Y = int.Parse(t[2]);

                    RiverDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }
            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 52)
                return false;

            return true;
        }
        private void Lire_River()
        {
            CarteCourante = "";

            foreach (var carte in RiverDetect)
            {
                // =====================================
                // Sécurité
                // =====================================

                if (string.IsNullOrEmpty(carte.Nom))
                    continue;

                // =====================================
                // Lecture pixel
                // =====================================

                System.Drawing.Color pixel = Bitmap_Main.GetPixel(carte.X, carte.Y);

                int couleurLue = (pixel.R << 16) | (pixel.G << 8) | pixel.B;

                // =====================================
                // Correspondance EXACTE
                // =====================================

                if (couleurLue == carte.Couleur)
                {
                    CarteCourante = carte.Nom;
                    AfficherCarte_Test(TB_R, Bt_River, 6);

                    return;
                }
            }
        }
        private void Sauver_River()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "River.json");
            string json = JsonConvert.SerializeObject(RiverDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 RIVER_HB
        // =========================================================
        private void Charger_River_HB_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "River_HB.json");
            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);
            River_HBDetect = JsonConvert.DeserializeObject<S_CarteDetect[]>(json);
        }
        private bool Extraire_River_HB(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;

            bool bRiver_HB = false;

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // COMMENT : River_HB
                // =====================================

                if (ligne == "COMMENT : River_HB")
                {
                    bRiver_HB = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================

                if (!bRiver_HB)
                    continue;

                // =====================================
                // COMMENT : Carte
                // =====================================

                if (ligne.StartsWith("COMMENT :"))
                {
                    string nom = ligne.Replace("COMMENT :", "").Trim();

                    if (nom == "River_HB")
                        continue;

                    if (nom.Length < 2)
                        continue;

                    // =====================================
                    // Ligne suivante
                    // =====================================

                    if (i + 1 >= lignes.Length)
                        continue;

                    string l2 = lignes[i + 1].Trim();

                    if (!l2.StartsWith("IF PIXEL COLOR EQUALS"))
                        continue;

                    // =====================================
                    // Découpage
                    // =====================================

                    string[] t = l2.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =====================================
                    // Extraction
                    // =====================================

                    River_HBDetect[index].Nom = nom;

                    River_HBDetect[index].X = int.Parse(t[1]);

                    River_HBDetect[index].Y = int.Parse(t[2]);

                    River_HBDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 104)
                return false;

            return true;
        }
        private void Lire_River_HB()
        {
            CarteCourante = "";

            foreach (var carte in River_HBDetect)
            {
                // =====================================
                // Sécurité
                // =====================================

                if (string.IsNullOrEmpty(carte.Nom))
                    continue;

                // =====================================
                // Lecture pixel
                // =====================================

                System.Drawing.Color pixel = Bitmap_Main.GetPixel(carte.X, carte.Y);

                int couleurLue = (pixel.R << 16) | (pixel.G << 8) | pixel.B;

                // =====================================
                // Correspondance EXACTE
                // =====================================

                if (couleurLue == carte.Couleur)
                {
                    CarteCourante = carte.Nom;
                    AfficherCarte_Test(TB_R, Bt_River, 6);

                    return;
                }
            }
        }
        private void Sauver_River_HB()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            String fichier = Path.Combine(DOSSIER_JSON, "River_HB.json");
            string json = JsonConvert.SerializeObject(River_HBDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 AMITIE
        // =========================================================
        private void Charger_Amitie_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Amitie.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            AmitieDetect = JsonConvert.DeserializeObject<S_AmitieDetect[]>(json);
        }
        private bool Extraire_Amitie(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bAmitie = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Amitie")
                {
                    bAmitie = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bAmitie)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    AmitieDetect[index].Nom = nomCourant;
                    AmitieDetect[index].X = int.Parse(t[1]);
                    AmitieDetect[index].Y = int.Parse(t[2]);
                    AmitieDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }


            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 1)
                return false;

            return true;
        }
        private void Sauver_Amitie()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Amitie.json");
            string json = JsonConvert.SerializeObject(AmitieDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }
        private bool Fermer_Amitie()
        {
            for (int i = 0; i < AmitieDetect.Length; i++)
            {
                int couleur = LirePixel(
                    AmitieDetect[i].X,
                    AmitieDetect[i].Y);

                if (couleur == AmitieDetect[i].Couleur)
                {
                    Clic(
                        AmitieDetect[i].X,
                        AmitieDetect[i].Y);

                    return true;
                }
            }

            return false;
        }
        // =========================================================
        // 🔥 AUCUNE TABLE
        // =========================================================
        private void Charger_Aucune_Table_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Aucune_Table.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            AucuneTableDetect = JsonConvert.DeserializeObject<S_AucuneTableDetect[]>(json);
        }// =========================================================
        // 🔥 CARTOGRAPHIE DU POT
        // =========================================================
        private void Charger_Cartographie_Du_Pot_JSON()
        {
            string fichier =
                Path.Combine(
                    DOSSIER_JSON,
                    "Cartographie_Du_Pot.json");

            if (!File.Exists(fichier))
                return;

            string json =
                File.ReadAllText(fichier);

            var temp =
                System.Text.Json.JsonSerializer
                    .Deserialize<
                        Dictionary<int, Cartographie_Longueur>
                    >(json);

            if (temp == null)
                return;

            CartographieDuPot = temp;
        }
        private bool Extraire_Cartographie_Du_Pot(string txt)
        {
            if (string.IsNullOrWhiteSpace(txt))
                return false;

            string[] lignes = txt.Split('\n');

            int longueur = 0;

            // =====================================
            // C# 8.0 : pas de ?
            // =====================================
            Cartographie_Longueur cartographieActuelle = null;

            string positionNom = "";

            bool premierPixel = false;
            bool trouve = false;

            int pixelX = 0;
            int pixelY = 0;
            int pixelCouleur = 0;

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =================================================
                // DÉBUT CARTOGRAPHIE DU POT
                // =================================================
                if (ligne == "COMMENT : Cartographie_Du_Pot")
                {
                    trouve = true;
                    continue;
                }

                // =================================================
                // NOUVELLE LONGUEUR
                // LABEL : 10 CHIFFRES
                // LABEL : 9 CHIFFRES
                // ...
                // =================================================
                if (ligne.StartsWith("LABEL : ") &&
                    ligne.EndsWith(" CHIFFRES"))
                {
                    string texte = ligne
                        .Replace("LABEL : ", "")
                        .Replace(" CHIFFRES", "")
                        .Trim();

                    if (!int.TryParse(texte, out longueur))
                        continue;

                    if (longueur < 1 || longueur > 10)
                        continue;

                    cartographieActuelle =
                        new Cartographie_Longueur
                        {
                            NombreChiffres = longueur
                        };

                    CartographieDuPot[longueur] =
                        cartographieActuelle;

                    positionNom = "";
                    premierPixel = false;

                    continue;
                }

                // =================================================
                // POSITION
                // =================================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    string nom =
                        ligne
                            .Replace("COMMENT :", "")
                            .Trim();

                    if (nom == "UNITE" ||
                        nom == "DIZAINE" ||
                        nom == "CENTAINE" ||
                        nom == "MILLIER" ||
                        nom == "DIZAINE MILLIER" ||
                        nom == "CENTAINE MILLIER" ||
                        nom == "MILLION" ||
                        nom == "DIZAINE MILLION" ||
                        nom == "CENTAINE MILLION" ||
                        nom == "MILLIARD")
                    {
                        positionNom = nom;

                        if (cartographieActuelle != null)
                        {
                            cartographieActuelle.Positions.Add(
                                new Cartographie_Position
                                {
                                    Nom = positionNom
                                });
                        }
                    }

                    continue;
                }

                // =================================================
                // IF PIXEL COLOR EQUALS
                // =================================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS :"))
                {
                    string[] morceaux =
                        ligne.Split(':');

                    if (morceaux.Length < 4)
                        continue;

                    if (!int.TryParse(
                            morceaux[1].Trim(),
                            out int x))
                        continue;

                    if (!int.TryParse(
                            morceaux[2].Trim(),
                            out int y))
                        continue;

                    if (!int.TryParse(
                            morceaux[3].Trim(),
                            out int couleur))
                        continue;

                    // =============================================
                    // PREMIER PIXEL
                    // PIXEL QUI DÉTERMINE LE NOMBRE DE CHIFFRES
                    // =============================================
                    if (cartographieActuelle != null &&
                        !premierPixel)
                    {
                        cartographieActuelle.PixelX = x;
                        cartographieActuelle.PixelY = y;
                        cartographieActuelle.PixelCouleur = couleur;

                        premierPixel = true;

                        continue;
                    }

                    // =============================================
                    // PIXEL D'UNE SIGNATURE DE CHIFFRE
                    // =============================================
                    pixelX = x;
                    pixelY = y;
                    pixelCouleur = couleur;

                    continue;
                }

                // =================================================
                // SET CLIPBOARD
                // =================================================
                if (ligne.StartsWith("SET CLIPBOARD :"))
                {
                    if (cartographieActuelle == null)
                        continue;

                    if (string.IsNullOrEmpty(positionNom))
                        continue;

                    string[] morceaux =
                        ligne.Split(':');

                    if (morceaux.Length < 2)
                        continue;

                    string valeur =
                        morceaux[1].Trim();

                    if (valeur.Length != 1 ||
                        !char.IsDigit(valeur[0]))
                        continue;

                    int chiffre =
                        valeur[0] - '0';

                    // =============================================
                    // RECHERCHE DE LA POSITION COURANTE
                    // =============================================
                    Cartographie_Position position =
                        cartographieActuelle.Positions
                            .LastOrDefault(
                                p => p.Nom == positionNom);

                    if (position == null)
                        continue;

                    // =============================================
                    // AJOUT DE LA SIGNATURE
                    // =============================================
                    position.Signatures.Add(
                        new Signature_Chiffre
                        {
                            Chiffre = chiffre,
                            X = pixelX,
                            Y = pixelY,
                            Couleur = pixelCouleur
                        });
                }
            }

            // =====================================================
            // RÉSULTAT
            // =====================================================
            return trouve &&
                   CartographieDuPot.Count > 0;
        }
        private void Sauver_Cartographie_Du_Pot()
        {
            Directory.CreateDirectory(DOSSIER_JSON);

            string fichier =
                Path.Combine(
                    DOSSIER_JSON,
                    "Cartographie_Du_Pot.json");

            string json =
                System.Text.Json.JsonSerializer.Serialize(
                    CartographieDuPot,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                fichier,
                json);
        }
        private bool Conformite_Cartographie_Du_Pot(string montant)
        {
            if (string.IsNullOrWhiteSpace(montant))
                return false;

            // =====================================
            // LONGUEUR DU MONTANT
            // =====================================
            int longueur = montant.Length;

            if (!CartographieDuPot.ContainsKey(longueur))
                return false;

            Cartographie_Longueur cartographie =
                CartographieDuPot[longueur];

            // =====================================
            // LONGUEUR INCOHÉRENTE
            // =====================================
            if (cartographie.NombreChiffres != longueur)
                return false;

            // =====================================
            // CHAQUE POSITION
            // =====================================
            for (int i = 0; i < montant.Length; i++)
            {
                char lu = montant[i];

                // =====================================
                // X = chiffre non identifié
                // On conserve la précision connue
                // =====================================
                if (lu == 'X')
                    continue;

                if (!char.IsDigit(lu))
                    return false;

                // =====================================
                // RECHERCHE DE LA POSITION
                // =====================================
                if (i >= cartographie.Positions.Count)
                    return false;

                Cartographie_Position position =
                    cartographie.Positions[i];

                // =====================================
                // SIGNATURE DU CHIFFRE
                // =====================================
                int chiffre =
                    lu - '0';

                Signature_Chiffre signature =
                    position.Signatures
                        .FirstOrDefault(
                            s => s.Chiffre == chiffre);

                // =====================================
                // SIGNATURE INCONNUE
                // =====================================
                if (signature == null)
                    return false;

                // =====================================
                // LECTURE DU PIXEL
                // =====================================
                int couleur =
                    LirePixel(
                        signature.X,
                        signature.Y);

                // =====================================
                // COULEUR NON CONFORME
                // =====================================
                if (couleur != signature.Couleur)
                    return false;
            }

            return true;
        }
        private bool Extraire_Aucune_Table(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bAucune_Table = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Aucune_Table")
                {
                    bAucune_Table = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bAucune_Table)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    AucuneTableDetect[index].Nom = nomCourant;
                    AucuneTableDetect[index].X = int.Parse(t[1]);
                    AucuneTableDetect[index].Y = int.Parse(t[2]);
                    AucuneTableDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }


            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 1)
                return false;

            return true;
        }
        private void Sauver_Aucune_Table()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Aucune_Table.json");
            string json = JsonConvert.SerializeObject(AucuneTableDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }
        private string Lire_Montant_Du_Pot()
        {
            if (CartographieDuPot == null || CartographieDuPot.Count == 0)
            {
                return "";
            }

            // =========================================================
            // 🔥 DÉTERMINATION DU NOMBRE DE CHIFFRES
            //
            // On part de 10 chiffres.
            //
            // 10 → tester détecteur 9
            //  9 → tester détecteur 8
            //  8 → tester détecteur 7
            //  7 → tester détecteur 6
            //  6 → tester détecteur 5
            //  5 → tester détecteur 4
            //  4 → tester détecteur 3
            //  3 → tester détecteur 2
            //  2 → tester détecteur 1
            //  1 → fin
            //
            // Le détecteur trouvé indique la longueur vers
            // laquelle on descend.
            // =========================================================
            int nombreChiffres = 10;

            for (int test = 9;
                 test >= 1;
                 test--)
            {
                if (!CartographieDuPot.ContainsKey(test))
                    continue;

                Cartographie_Longueur cartographieTest = CartographieDuPot[test];

                int couleur = LirePixel(cartographieTest.PixelX, cartographieTest.PixelY);

                if (couleur == cartographieTest.PixelCouleur)
                {
                    nombreChiffres = test;

                    break;
                }
            }

            // =========================================================
            // 🔥 AUCUN DÉTECTEUR TROUVÉ
            // → 10 CHIFFRES
            // =========================================================
            if (nombreChiffres == 10)
            {
                Debug( "🔥 AUCUN DETECTEUR 9 A 1 -> 10 CHIFFRES");
            }

            // =========================================================
            // 🔥 CARTOGRAPHIE CORRESPONDANTE
            // =========================================================
            if (!CartographieDuPot.ContainsKey(nombreChiffres))
            {
                Debug("❌ CARTOGRAPHIE " + nombreChiffres + " CHIFFRES ABSENTE");

                return "";
            }

            Cartographie_Longueur cartographie = CartographieDuPot[nombreChiffres];

            // =========================================================
            // 🔥 AFFICHAGE DE LA LONGUEUR
            // =========================================================
            TB_Montant.Foreground = Brushes.Orange;

            TB_Montant.Text = nombreChiffres.ToString() + " chiffres";

            // =========================================================
            // 🔥 LECTURE DES POSITIONS
            // =========================================================
            char[] montant = new char[nombreChiffres];

            for (int i = 0;
                 i < nombreChiffres;
                 i++)
            {
                char chiffreTrouve = 'X';

                // =====================================
                // POSITION DISPONIBLE ?
                // =====================================
                if (i < cartographie.Positions.Count)
                {
                    Cartographie_Position position = cartographie.Positions[i];

                    // =====================================
                    // TEST DES SIGNATURES
                    // =====================================
                    for (int j = 0;
                         j < position.Signatures.Count;
                         j++)
                    {
                        Signature_Chiffre signature = position.Signatures[j];

                        int couleur = LirePixel(signature.X, signature.Y);

                        if (couleur == signature.Couleur)
                        {
                            chiffreTrouve = (char)('0' + signature.Chiffre);

                            break;
                        }
                    }
                }

                // =====================================
                // UNITÉ = DERNIER CARACTÈRE
                // DIZAINE = AVANT-DERNIER
                // =====================================
                int indexFinal = nombreChiffres - 1 - i;

                montant[indexFinal] = chiffreTrouve;
            }

            string resultat = new string(montant);

            // =========================================================
            // 🔥 MONTANT FINAL
            // =========================================================
            if (resultat.Contains("X"))
            {
                TB_Montant.Foreground = Brushes.Orange;
            }
            else
            {
                TB_Montant.Foreground = Brushes.Green;
            }

            TB_Montant.Text = resultat;

            return resultat;
        }
        private bool bTestPot = false;
        private void Bt_Test_Pot_Click(object sender, RoutedEventArgs e)
        {
            CaptureEcran();

            int longueur = Detecter_Longueur_Pot();

            TB_Montant.Text = longueur.ToString();

            // =====================================
            // 🟢 BOUTON ACTIF
            // =====================================
            Bt_Test_Pot.Background = Brushes.LightGreen;
        }
        private int Detecter_Longueur_Pot()
        {
            foreach (var item in CartographieDuPot)
            {
                int nombreChiffres = item.Key;

                Cartographie_Longueur cartographie = item.Value;

                int couleur = LirePixel(cartographie.PixelX, cartographie.PixelY);

                if (couleur == cartographie.PixelCouleur)
                {
                    return nombreChiffres;
                }
            }

            return 0;
        }

        // =========================================================
        // 🔥 DECISION
        // =========================================================
        private void Charger_Decision_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Decision.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            S_DecisionDetect[] detecteurs =
                JsonConvert.DeserializeObject<S_DecisionDetect[]>(json);

            if (detecteurs == null || detecteurs.Length != 5)
                return;

            DecisionDetect = detecteurs;
        }
        private bool Extraire_Decision(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;

            bool bDecision = false;
            string nomCourant = "";

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Decision")
                {
                    bDecision = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bDecision)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant =
                        ligne.Replace("COMMENT :", "").Trim();

                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    // =================================
                    // SECURITE : MAXIMUM 5 DETECTEURS
                    // =================================
                    if (index >= DecisionDetect.Length)
                        return false;

                    string[] t = ligne.Split(':');

                    if (t.Length < 4)
                        return false;

                    DecisionDetect[index].Nom = nomCourant;
                    DecisionDetect[index].X = int.Parse(t[1]);
                    DecisionDetect[index].Y = int.Parse(t[2]);
                    DecisionDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // EXACTEMENT 5 DETECTEURS
            // =====================================
            if (index != 5)
                return false;

            return true;
        }
        private void Sauver_Decision()
        {
            Directory.CreateDirectory(DOSSIER_JSON);

            string fichier =
                Path.Combine(DOSSIER_JSON, "Decision.json");

            string json =
                JsonConvert.SerializeObject(
                    DecisionDetect,
                    Formatting.Indented);

            File.WriteAllText(fichier, json);
        }
        private bool Decision()
        {
            switch (ActionForcee)
            {
                case "PAROLE":

                    // =====================================
                    // PAROLE DISPONIBLE
                    // =====================================
                    if (Decision_Detecte("Parole"))
                    {
                        JouerSon("Parole.wav");
                        return true;
                    }

                    // =====================================
                    // SINON SE COUCHER
                    // =====================================
                    if (Decision_Detecte("Se_Coucher"))
                    {
                        return true;
                    }

                    break;


                case "SUIVRE":

                    if (Decision_Detecte("Suivre"))
                    {
                        return true;
                    }

                    break;


                case "RELANCER":

                    if (Decision_Detecte("Relancer"))
                    {
                        return true;
                    }

                    break;


                case "TAPIS":

                    if (Decision_Detecte("Tapis"))
                    {
                        JouerSon("Tapis.wav");
                        return true;
                    }

                    break;
            }

            return false;
        }
        private bool bPasse_Annonce = false;
        private bool Decision_Detecte(string nom)
        {
            for (int i = 0; i < DecisionDetect.Length; i++)
            {
                if (DecisionDetect[i].Nom != nom)
                    continue;

                int couleur =
                    LirePixel(
                        DecisionDetect[i].X,
                        DecisionDetect[i].Y);

                if (couleur == DecisionDetect[i].Couleur)
                {
                    // =====================================
                    // 🔊 PASSE : UNE SEULE FOIS PAR MAIN
                    // =====================================
                    if (nom == "Se_Coucher" && !bPasse_Annonce)
                    {
                        bPasse_Annonce = true;

                        JouerSon("Passe.wav");

                        return false;
                    }

                    // =====================================
                    // 🔥 UN SEUL CLIC
                    // =====================================
                    Clic(
                        DecisionDetect[i].X,
                        DecisionDetect[i].Y);
                    // =====================================
                    // 🔥 ENLEVER LE HOVER
                    // =====================================
                    DeplacerSouris(1109, 837);

                    // =====================================
                    // 🔒 ATTENDRE LA DISPARITION
                    // =====================================
                    bAttente_Disparition = true;
                    Nom_Bouton_Attente = nom;

                    return true;
                }
            }

            return false;
        }
        private bool Bouton_Decision_Present(string nom)
        {
            for (int i = 0; i < DecisionDetect.Length; i++)
            {
                if (DecisionDetect[i].Nom != nom)
                    continue;

                int couleur =
                    LirePixel(
                        DecisionDetect[i].X,
                        DecisionDetect[i].Y);

                return couleur == DecisionDetect[i].Couleur;
            }


            return false;
        }
        private bool Mode_Decision()
        {
            bool resultat =
                Bouton_Decision_Present("Se_Coucher");

            return resultat;
        }
        private bool Decision_Autorisee()
        {

            // ==========================
            // MODE MANUEL
            // ==========================
            if (bModeManuel)
            {
                return false;
            }

            // ==========================
            // DELAI PRISE DE CONTROLE
            // ==========================
            double temps =
                (DateTime.Now - Heure_Decision).TotalMilliseconds;

            if (temps < DELAI_PRISE_CONTROLE)
            {
                return false;
            }

            return true;
        }
        private void Sl_Delai_Decision_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded)
                return;

            DELAI_PRISE_CONTROLE =
                Convert.ToInt32(e.NewValue * 1000);

            T_Delai_Decision.Text =
                Sl_Delai_Decision.Value + " s";
        }
        private void Tapis_PreFlop_Decision()
        {
            if (!bTapisPreFlop_Actif)
            {
                Bt_Tapis.Background = Blanc;
                return;
            }

            string main = NormaliserMainTapis(T_Carte[0], T_Carte[1]);

            if (string.IsNullOrEmpty(main))
            {
                Bt_Tapis.Background = Blanc;
                return;
            }

            bool dansRange = EstDansRange(main);

            Bt_Tapis.Background =
                dansRange
                    ? Vert
                    : Blanc;
        }
        private void AppliquerActionForcee()
        {
            // =====================================
            // Reset
            // =====================================
            Bt_Tapis.Background = Blanc;
            Bt_Parole_Passe.Background = Blanc;
            Bt_Suivre.Background = Blanc;
            Bt_Relancer.Background = Blanc;

            // =====================================
            // Action sélectionnée
            // =====================================
            switch (ActionForcee)
            {
                case "TAPIS":
                    Bt_Tapis.Background = Vert;
                    break;

                case "PAROLE":
                    Bt_Parole_Passe.Background = Vert;
                    break;

                case "SUIVRE":
                    Bt_Suivre.Background = Vert;
                    break;

                case "RELANCER":
                    Bt_Relancer.Background = Vert;
                    break;
            }
        }

        // =========================================================
        // 🔥 FELICITATION
        // =========================================================
        private void Charger_Felicitation_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Felicitation.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            FelicitationDetect =
                JsonConvert.DeserializeObject<S_FelicitationDetect[]>(json);
        }
        private bool Extraire_Felicitation(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;
            bool bFelicitation = false;
            string nomCourant = "";

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Felicitation")
                {
                    bFelicitation = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bFelicitation)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant =
                        ligne.Replace("COMMENT :", "").Trim();

                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =====================================
                    // 🔥 PROTECTION
                    // =====================================
                    if (index >= FelicitationDetect.Length)
                        continue;

                    FelicitationDetect[index].Nom = nomCourant;
                    FelicitationDetect[index].X = int.Parse(t[1]);
                    FelicitationDetect[index].Y = int.Parse(t[2]);
                    FelicitationDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // 🔥 DEUX PIXELS OBLIGATOIRES
            // =====================================
            if (index != 2)
                return false;

            return true;
        }
        private void Sauver_Felicitation()
        {
            Directory.CreateDirectory(DOSSIER_JSON);

            string fichier =
                Path.Combine(DOSSIER_JSON, "Felicitation.json");

            string json =
                JsonConvert.SerializeObject(
                    FelicitationDetect,
                    Formatting.Indented);

            File.WriteAllText(fichier, json);
        }
        private bool Fermer_Felicitation()
        {
            if (FelicitationDetect.Length < 2)
                return false;

            int couleur =
                LirePixel(
                    FelicitationDetect[1].X,
                    FelicitationDetect[1].Y);

            if (couleur == FelicitationDetect[1].Couleur)
            {
                Clic(
                    FelicitationDetect[1].X,
                    FelicitationDetect[1].Y);

                return true;
            }

            return false;
        }

        // =========================================================
        // 🔥 INFO JOUEUR
        // =========================================================
        private void Charger_Info_Joueur_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Info_Joueur.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            InfoJoueurDetect = JsonConvert.DeserializeObject<S_InfoJoueurDetect[]>(json);
        }
        private bool Extraire_Info_Joueur(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bInfoJoueur = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Info_Joueur")
                {
                    bInfoJoueur = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bInfoJoueur)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    InfoJoueurDetect[index].Nom = nomCourant;
                    InfoJoueurDetect[index].X = int.Parse(t[1]);
                    InfoJoueurDetect[index].Y = int.Parse(t[2]);
                    InfoJoueurDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 1)
                return false;

            return true;
        }
        private void Sauver_Info_Joueur()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Info_Joueur.json");
            string json = JsonConvert.SerializeObject(InfoJoueurDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }
        // =========================================================
        // 🔥 INFO JOUEUR
        // Attendre 2 secondes avant fermeture
        // =========================================================
        private bool bInfoJoueurAttente = false;
        private DateTime HeureInfoJoueur = DateTime.MinValue;
        private bool Fermer_Info_Joueur()
        {
            for (int i = 0; i < InfoJoueurDetect.Length; i++)
            {
                int couleur = LirePixel(
                    InfoJoueurDetect[i].X,
                    InfoJoueurDetect[i].Y);

                if (couleur != InfoJoueurDetect[i].Couleur)
                    continue;

                // =====================================
                // PREMIÈRE DÉTECTION
                // =====================================
                if (!bInfoJoueurAttente)
                {
                    bInfoJoueurAttente = true;
                    HeureInfoJoueur = DateTime.Now;

                    return false;
                }

                // =====================================
                // ATTENDRE 2 SECONDES
                // =====================================
                if ((DateTime.Now - HeureInfoJoueur).TotalSeconds < 2)
                    return false;

                // =====================================
                // 2 SECONDES ÉCOULÉES
                // → FERMER
                // =====================================
                Clic(
                    InfoJoueurDetect[i].X,
                    InfoJoueurDetect[i].Y);

                bInfoJoueurAttente = false;
                HeureInfoJoueur = DateTime.MinValue;

                return true;
            }

            // =====================================
            // FENÊTRE ABSENTE
            // RESET DE L'ATTENTE
            // =====================================
            bInfoJoueurAttente = false;
            HeureInfoJoueur = DateTime.MinValue;

            return false;
        }// Laisser 2 secondes pour lire

        // =========================================================
        // 🔥 JE NE SUIS PAS ASSIS
        // =========================================================
        private void Charger_Je_Ne_Suis_Pas_Assis_JSON()
        {
            string fichier = Path.Combine(
                DOSSIER_JSON,
                "Je_Ne_Suis_Pas_Assis.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            JeNeSuisPasAssisDetect =
                JsonConvert.DeserializeObject<S_JeNeSuisPasAssisDetect[]>(json);
        }
        private bool Extraire_Je_Ne_Suis_Pas_Assis(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bJeNeSuisPasAssis = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Je_Ne_Suis_Pas_Assis")
                {
                    bJeNeSuisPasAssis = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bJeNeSuisPasAssis)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    JeNeSuisPasAssisDetect[index].Nom = nomCourant;
                    JeNeSuisPasAssisDetect[index].X = int.Parse(t[1]);
                    JeNeSuisPasAssisDetect[index].Y = int.Parse(t[2]);
                    JeNeSuisPasAssisDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 1)
                return false;

            return true;
        }
        private void Sauver_Je_Ne_Suis_Pas_Assis()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Je_Ne_Suis_Pas_Assis.json");
            string json = JsonConvert.SerializeObject(JeNeSuisPasAssisDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }
        // =========================================================
        // 🔥 J'ai quitté OU je ne suis pas encore assis
        // =========================================================
        private bool Je_Ne_Suis_Pas_Assis()
        {
            if (JeNeSuisPasAssisDetect == null || JeNeSuisPasAssisDetect.Length == 0)
                return false;

            int couleur = LirePixel(JeNeSuisPasAssisDetect[0].X, JeNeSuisPasAssisDetect[0].Y);

            return couleur == JeNeSuisPasAssisDetect[0].Couleur;
        }

        // =========================================================
        // 🔥 JOUEUR A TAPIS
        // =========================================================
        private void Charger_Joueur_A_Tapis_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_A_Tapis.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            JoueurATapisDetect = JsonConvert.DeserializeObject<S_JoueurATapisDetect[]>(json);
        }
        private bool Extraire_Joueur_A_Tapis(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bJoueur_A_Tapis = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Joueur_A_Tapis")
                {
                    bJoueur_A_Tapis = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bJoueur_A_Tapis)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    JoueurATapisDetect[index].Nom = nomCourant;
                    JoueurATapisDetect[index].X = int.Parse(t[1]);
                    JoueurATapisDetect[index].Y = int.Parse(t[2]);
                    JoueurATapisDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 9)
                return false;

            return true;
        }
        private void Sauver_Joueur_A_Tapis()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_A_Tapis.json");
            string json = JsonConvert.SerializeObject(JoueurATapisDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 JOUEUR PAROLE
        // =========================================================
        private void Charger_Joueur_Parole_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_Parole.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            JoueurParoleDetect = JsonConvert.DeserializeObject<S_JoueurParoleDetect[]>(json);
        }
        private bool Extraire_Joueur_Parole(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bJoueurParole = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Joueur_Parole")
                {
                    bJoueurParole = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bJoueurParole)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    JoueurParoleDetect[index].Nom = nomCourant;
                    JoueurParoleDetect[index].X = int.Parse(t[1]);
                    JoueurParoleDetect[index].Y = int.Parse(t[2]);
                    JoueurParoleDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 9)
                return false;

            return true;
        }
        private void Sauver_Joueur_Parole()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_Parole.json");
            string json = JsonConvert.SerializeObject(JoueurParoleDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 JOUEUR REFLECHIT
        // =========================================================
        private void Charger_Joueur_Reflechit_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_Reflechit.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            JoueurReflechitDetect = JsonConvert.DeserializeObject<S_JoueurReflechitDetect[]>(json);
        }
        private bool Extraire_Joueur_Reflechit(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bJoueur_Reflechit = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Joueur_Reflechit")
                {
                    bJoueur_Reflechit = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bJoueur_Reflechit)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    JoueurReflechitDetect[index].Nom = nomCourant;
                    JoueurReflechitDetect[index].X = int.Parse(t[1]);
                    JoueurReflechitDetect[index].Y = int.Parse(t[2]);
                    JoueurReflechitDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 9)
                return false;

            return true;
        }
        private void Sauver_Joueur_Reflechit()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_Reflechit.json");
            string json = JsonConvert.SerializeObject(JoueurReflechitDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 JOUEUR RELANCE
        // =========================================================
        private void Charger_Joueur_Relance_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_Relance.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            JoueurRelanceDetect = JsonConvert.DeserializeObject<S_JoueurRelanceDetect[]>(json);
        }
        private bool Extraire_Joueur_Relance(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bJoueur_Relance = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Joueur_Relance")
                {
                    bJoueur_Relance = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bJoueur_Relance)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    JoueurRelanceDetect[index].Nom = nomCourant;
                    JoueurRelanceDetect[index].X = int.Parse(t[1]);
                    JoueurRelanceDetect[index].Y = int.Parse(t[2]);
                    JoueurRelanceDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 9)
                return false;

            return true;
        }
        private void Sauver_Joueur_Relance()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_Relance.json");
            string json = JsonConvert.SerializeObject(JoueurRelanceDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 JOUEUR SE COUCHE
        // =========================================================
        private void Charger_Joueur_Se_Couche_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_Se_Couche.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            JoueurSeCoucheDetect = JsonConvert.DeserializeObject<S_JoueurSeCoucheDetect[]>(json);
        }
        private bool Extraire_Joueur_Se_Couche(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bJoueur_Se_Couche = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Joueur_Se_Couche")
                {
                    bJoueur_Se_Couche = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bJoueur_Se_Couche)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    JoueurSeCoucheDetect[index].Nom = nomCourant;
                    JoueurSeCoucheDetect[index].X = int.Parse(t[1]);
                    JoueurSeCoucheDetect[index].Y = int.Parse(t[2]);
                    JoueurSeCoucheDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 9)
                return false;

            return true;
        }
        private void Sauver_Joueur_Se_Couche()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_Se_Couche.json");
            string json = JsonConvert.SerializeObject(JoueurSeCoucheDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 JOUEUR SUIT
        // =========================================================
        private void Charger_Joueur_Suit_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_Suit.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            JoueurSuitDetect = JsonConvert.DeserializeObject<S_JoueurSuitDetect[]>(json);
        }
        private bool Extraire_Joueur_Suit(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bJoueur_Suit = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Joueur_Suit")
                {
                    bJoueur_Suit = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bJoueur_Suit)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    JoueurSuitDetect[index].Nom = nomCourant;
                    JoueurSuitDetect[index].X = int.Parse(t[1]);
                    JoueurSuitDetect[index].Y = int.Parse(t[2]);
                    JoueurSuitDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 9)
                return false;

            return true;
        }
        private void Sauver_Joueur_Suit()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_Suit.json");
            string json = JsonConvert.SerializeObject(JoueurSuitDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 GAGNE
        // =========================================================
        private void Charger_Gagne_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Gagne.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            GagneDetect =
                JsonConvert.DeserializeObject<S_GagneDetect[]>(json);
        }
        private bool Extraire_Gagne(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;
            bool bGagne = false;
            int nomCourant = 0;

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Gagne")
                {
                    bGagne = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bGagne)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant =
                        int.Parse(
                            ligne.Replace("COMMENT :", "").Trim());

                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =====================================
                    // SECURITE : TABLEAU PLEIN
                    // =====================================
                    if (index >= GagneDetect.Length)
                        return false;

                    GagneDetect[index].Nom = nomCourant;
                    GagneDetect[index].X = int.Parse(t[1]);
                    GagneDetect[index].Y = int.Parse(t[2]);
                    GagneDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // 9 POSITIONS × 2 PIXELS
            // =====================================
            return index == 18;
        }
        private void Sauver_Gagne()
        {
            Directory.CreateDirectory(DOSSIER_JSON);

            string fichier =
                Path.Combine(DOSSIER_JSON, "Gagne.json");

            string json =
                JsonConvert.SerializeObject(
                    GagneDetect,
                    Formatting.Indented);

            File.WriteAllText(fichier, json);
        }
        private List<int> Gagnants()
        {
            List<int> gagnants = new List<int>();

            for (int i = 0; i < GagneDetect.Length; i++)
            {
                int couleur = LirePixel(
                    GagneDetect[i].X,
                    GagneDetect[i].Y);

                if (couleur == GagneDetect[i].Couleur)
                {
                    if (!gagnants.Contains(GagneDetect[i].Nom))
                    {
                        gagnants.Add(GagneDetect[i].Nom);

                        // =====================================
                        // 🔥 AFFICHER LE GAGNANT
                        // =====================================
                        switch (GagneDetect[i].Nom)
                        {
                            case 1:
                                Bt_PlaceDisponible_1.Background = Brushes.Red;
                                break;

                            case 2:
                                Bt_PlaceDisponible_2.Background = Brushes.Red;
                                break;

                            case 3:
                                Bt_PlaceDisponible_3.Background = Brushes.Red;
                                break;

                            case 4:
                                Bt_PlaceDisponible_4.Background = Brushes.Red;
                                break;

                            case 5:
                                Bt_PlaceDisponible_5.Background = Brushes.Red;
                                break;

                            case 6:
                                Bt_PlaceDisponible_6.Background = Brushes.Red;
                                break;

                            case 7:
                                Bt_PlaceDisponible_7.Background = Brushes.Red;
                                break;

                            case 8:
                                Bt_PlaceDisponible_8.Background = Brushes.Red;
                                break;

                            case 9:
                                Bt_PlaceDisponible_9.Background = Brushes.Red;
                                break;
                        }
                    }
                }
            }

            return gagnants;
        }

        private bool J_Ai_Gagne()
        {
            List<int> gagnants = Gagnants();

            return gagnants.Contains(MaPlace);
        }
        // =====================================================
        // 🔥 EFFACER LES GAGNANTS
        // À appeler à chaque nouvelle Main1
        // =====================================================
        private void Effacer_Gagnants()
        {
            Bt_PlaceDisponible_1.Background = Brushes.White;
            Bt_PlaceDisponible_2.Background = Brushes.White;
            Bt_PlaceDisponible_3.Background = Brushes.White;
            Bt_PlaceDisponible_4.Background = Brushes.White;
            Bt_PlaceDisponible_5.Background = Brushes.White;
            Bt_PlaceDisponible_6.Background = Brushes.White;
            Bt_PlaceDisponible_7.Background = Brushes.White;
            Bt_PlaceDisponible_8.Background = Brushes.White;
            Bt_PlaceDisponible_9.Background = Brushes.White;
        }
        // =========================================================
        // 🔥 MAIN BOARD
        // =========================================================
        private void Charger_Main_Board_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Main_Board.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            MainBoardDetect = JsonConvert.DeserializeObject<S_MainBoardDetect[]>(json);
        }
        private bool Extraire_Main_Board(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;

            bool bMain_Board = false;

            string nomCourant = "";

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================

                if (ligne == "COMMENT : Main_Board_Presence")
                {
                    bMain_Board = true;

                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================

                if (!bMain_Board)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================

                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();

                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================

                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');

                    if (t.Length < 5)
                        continue;

                    MainBoardDetect[index].Nom = nomCourant;

                    MainBoardDetect[index].X = int.Parse(t[1]);

                    MainBoardDetect[index].Y = int.Parse(t[2]);

                    MainBoardDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================
            if (index != 82)
            {
                return false;
            }

            return true;
        }
        private void Sauver_Main_Board()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            String fichier = Path.Combine(DOSSIER_JSON, "Main_Board.json");
            string json = JsonConvert.SerializeObject(MainBoardDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 NIVEAU TABLE
        // =========================================================
        private void Charger_Niveau_Table_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Niveau_Table.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            NiveauTableDetect = JsonConvert.DeserializeObject<S_NiveauTableDetect[]>(json);
        }
        private bool Extraire_Niveau_Table(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bNiveau_Table = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Niveau_Table")
                {
                    bNiveau_Table = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bNiveau_Table)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    NiveauTableDetect[index].Nom = nomCourant;
                    NiveauTableDetect[index].X = int.Parse(t[1]);
                    NiveauTableDetect[index].Y = int.Parse(t[2]);
                    NiveauTableDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 12)
                return false;

            return true;
        }
        private void Sauver_Niveau_Table()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Niveau_Table.json");
            string json = JsonConvert.SerializeObject(NiveauTableDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }
        private int Niveau_Table()
        {
            for (int i = 0; i < NiveauTableDetect.Length; i++)
            {
                int couleur = LirePixel(
                    NiveauTableDetect[i].X,
                    NiveauTableDetect[i].Y);

                if (couleur == NiveauTableDetect[i].Couleur)
                    return i + 1;     // 1 à 12
            }

            return 0;                 // Non détecté
        }
        private void Lire_Niveau_Table()
        {
            ResetNiveauTable();

            int niveau = Niveau_Table();

            if (niveau >= 1 && niveau <= 12)
            {
                ActiverNiveauTable(BoutonsNiveauTable[niveau - 1]);
                AfficherInformationsTable(niveau);
            }
        }
        private void AfficherInformationsTable(int niveau)
        {
            switch (niveau)
            {
                case 1:     // Greenhorn
                    T_Blinde_1.Text = "1";
                    T_Blinde_2.Text = "2";
                    T_Cave_Min.Text = "20";
                    T_Cave_Max.Text = "400";
                    T_Frais.Text = "1";
                    break;

                case 2:     // Beginner
                    T_Blinde_1.Text = "5";
                    T_Blinde_2.Text = "10";
                    T_Cave_Min.Text = "100";
                    T_Cave_Max.Text = "2000";
                    T_Frais.Text = "5";
                    break;

                case 3:     // Gambler
                    T_Blinde_1.Text = "25";
                    T_Blinde_2.Text = "50";
                    T_Cave_Min.Text = "500";
                    T_Cave_Max.Text = "10000";
                    T_Frais.Text = "25";
                    break;

                case 4:     // Pro
                    T_Blinde_1.Text = "100";
                    T_Blinde_2.Text = "200";
                    T_Cave_Min.Text = "2000";
                    T_Cave_Max.Text = "40000";
                    T_Frais.Text = "100";
                    break;

                case 5:     // BigFish
                    T_Blinde_1.Text = "500";
                    T_Blinde_2.Text = "1000";
                    T_Cave_Min.Text = "10000";
                    T_Cave_Max.Text = "200000";
                    T_Frais.Text = "500";
                    break;

                case 6:     // Master
                    T_Blinde_1.Text = "1000";
                    T_Blinde_2.Text = "2000";
                    T_Cave_Min.Text = "20000";
                    T_Cave_Max.Text = "400000";
                    T_Frais.Text = "1000";
                    break;

                case 7:     // Champion
                    T_Blinde_1.Text = "5000";
                    T_Blinde_2.Text = "10000";
                    T_Cave_Min.Text = "100000";
                    T_Cave_Max.Text = "2000000";
                    T_Frais.Text = "5000";
                    break;

                case 8:     // Guru
                    T_Blinde_1.Text = "25000";
                    T_Blinde_2.Text = "50000";
                    T_Cave_Min.Text = "500000";
                    T_Cave_Max.Text = "10000000";
                    T_Frais.Text = "25000";
                    break;

                case 9:     // Ultimate
                    T_Blinde_1.Text = "100000";
                    T_Blinde_2.Text = "200000";
                    T_Cave_Min.Text = "2M";
                    T_Cave_Max.Text = "40M";
                    T_Frais.Text = "100000";
                    break;

                case 10:    // Insane
                    T_Blinde_1.Text = "1M";
                    T_Blinde_2.Text = "2M";
                    T_Cave_Min.Text = "20M";
                    T_Cave_Max.Text = "400M";
                    T_Frais.Text = "1M";
                    break;

                case 11:    // Legendary
                    T_Blinde_1.Text = "2M";
                    T_Blinde_2.Text = "4M";
                    T_Cave_Min.Text = "40M";
                    T_Cave_Max.Text = "800M";
                    T_Frais.Text = "2M";
                    break;

                default:
                    T_Blinde_1.Text = "";
                    T_Blinde_2.Text = "";
                    T_Cave_Min.Text = "";
                    T_Cave_Max.Text = "";
                    T_Frais.Text = "";
                    break;
            }
        }

        // =========================================================
        // 🔥 NOUVEAU NIVEAU
        // =========================================================
        private void Charger_Nouveau_Niveau_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Nouveau_Niveau.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            PupitreGrisDetect = JsonConvert.DeserializeObject<S_PupitreGrisDetect[]>(json);
        }
        private bool Extraire_Nouveau_Niveau(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bNouveau_Niveau = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Nouveau_Niveau")
                {
                    bNouveau_Niveau = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bNouveau_Niveau)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    NouveauNiveauDetect[index].Nom = nomCourant;
                    NouveauNiveauDetect[index].X = int.Parse(t[1]);
                    NouveauNiveauDetect[index].Y = int.Parse(t[2]);
                    NouveauNiveauDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 1)
                return false;

            return true;
        }
        private void Sauver_Nouveau_Niveau()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Nouveau_Niveau.json");
            string json = JsonConvert.SerializeObject(NouveauNiveauDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 PLACES LIBRES
        // =========================================================
        private void Charger_Places_Libres_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Places_Libres.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            PlacesLibresDetect =
                JsonConvert.DeserializeObject<S_PlacesLibresDetect[]>(json);
        }
        private bool Extraire_Places_Libres(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;
            bool bPlaces_Libres = false;
            string nomCourant = "";

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Places_Libres")
                {
                    bPlaces_Libres = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bPlaces_Libres)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant =
                        ligne.Replace("COMMENT :", "").Trim();

                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =====================================
                    // 18 PIXELS MAXIMUM
                    // =====================================
                    if (index >= 18)
                        continue;

                    PlacesLibresDetect[index].Nom = nomCourant;

                    PlacesLibresDetect[index].X =
                        int.Parse(t[1]);

                    PlacesLibresDetect[index].Y =
                        int.Parse(t[2]);

                    PlacesLibresDetect[index].Couleur =
                        int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================
            if (index != 18)
                return false;

            return true;
        }
        private void Sauver_Places_Libres()
        {
            Directory.CreateDirectory(DOSSIER_JSON);

            string fichier =
                Path.Combine(DOSSIER_JSON, "Places_Libres.json");

            string json =
                JsonConvert.SerializeObject(
                    PlacesLibresDetect,
                    Formatting.Indented);

            File.WriteAllText(fichier, json);
        }
        private int Place_Libre()
        {
            for (int i = 0; i < 18; i += 2)
            {
                int c1 = LirePixel(
                    PlacesLibresDetect[i].X,
                    PlacesLibresDetect[i].Y);

                int c2 = LirePixel(
                    PlacesLibresDetect[i + 1].X,
                    PlacesLibresDetect[i + 1].Y);

                // =====================================
                // 🟠 UN OU L'AUTRE DES DEUX PIXELS
                // =====================================
                if (c1 == PlacesLibresDetect[i].Couleur ||
                    c2 == PlacesLibresDetect[i + 1].Couleur)
                {
                    return i / 2 + 1;
                }
            }

            return 0;
        }

        // =========================================================
        // 🔥 PLACE 1 LIBRE ?
        // =========================================================
        private bool Place_1_Libre()
        {
            int c1 = LirePixel(
                PlacesLibresDetect[0].X,
                PlacesLibresDetect[0].Y);

            int c2 = LirePixel(
                PlacesLibresDetect[1].X,
                PlacesLibresDetect[1].Y);

            // =====================================
            // 🟠 UN OU L'AUTRE DES DEUX PIXELS
            // =====================================
            if (c1 == PlacesLibresDetect[0].Couleur ||
                c2 == PlacesLibresDetect[1].Couleur)
            {
                return true;
            }

            return false;
        }
        private async Task S_Assoir_A_Table()
        {
            while (true)
            {
                // ==========================================
                // CHERCHER UNE PLACE LIBRE
                // ==========================================
                CaptureEcran();

                int place = Place_Libre();

                if (place == 0)
                {
                    await Task.Delay(5);
                    continue;
                }

                // ==========================================
                // CLIQUER UNE SEULE FOIS SUR LA PLACE
                // ==========================================
                Cliquer_Place(place);

                await Task.Delay(200);

                // ==========================================
                // ATTENDRE LE RÉSULTAT DU CLIC
                // ==========================================
                while (true)
                {
                    await Task.Delay(2);

                    CaptureEcran();

                    // ======================================
                    // 🔥 PLUS DE JETONS
                    // → RECAVE IMMEDIATE
                    // ======================================
                    if (Plus_De_Jetons())
                    {
                        await Task.Delay(100);
                        Se_Recaver();

                        return;
                    }

                    // ======================================
                    // ✅ JE SUIS ASSIS
                    // ======================================
                    int position = Suis_Je_Assis();

                    if (position != 0)
                    {

                        // ==================================
                        // ATTENDRE LA NOUVELLE STABILISATION
                        // ==================================
                        while (!Table_Stabilisee())
                        {
                            await Task.Delay(2);

                            CaptureEcran();
                        }

                        DeplacerSouris(1107, 837);

                        return;
                    }

                    // ======================================
                    // ❌ PLACE DEJA OCCUPÉE
                    // ======================================

                    await Task.Delay(100);
                    if (Fermer_Place_Occupee())
                    {
                        break;
                    }
                }
            }
        }
        private void Cliquer_Place(int place)
        {
            if (place < 1 || place > 9)
                return;

            // =====================================
            // DEBUT DE LA PAIRE DE PIXELS
            // =====================================
            int index = (place - 1) * 2;

            // =====================================
            // ON PEUT CLIQUER SUR L'UN OU L'AUTRE
            // =====================================
            int x = PlacesLibresDetect[index].X;
            int y = PlacesLibresDetect[index].Y;

            Clic(x, y);
            // =====================================
            // 🔥 SORTIR LA SOURIS DU PUPITRE
            // =====================================
            DeplacerSouris(1131, 855);
        }
        // =========================================================
        // 🔥 PLACE OCCUPEE
        // =========================================================
        private void Charger_Place_Occupee_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Place_Occupee.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            PlaceOccupeeDetect = JsonConvert.DeserializeObject<S_PlaceOccupeeDetect[]>(json);
        }
        private bool Extraire_Place_Occupee(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bPlace_Occupee = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Place_Occupee")
                {
                    bPlace_Occupee = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bPlace_Occupee)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    PlaceOccupeeDetect[index].Nom = nomCourant;
                    PlaceOccupeeDetect[index].X = int.Parse(t[1]);
                    PlaceOccupeeDetect[index].Y = int.Parse(t[2]);
                    PlaceOccupeeDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 1)
                return false;

            return true;
        }
        private void Sauver_Place_Occupee()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Place_Occupee.json");
            string json = JsonConvert.SerializeObject(PlaceOccupeeDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }
        private bool Fermer_Place_Occupee()
        {
            for (int i = 0; i < PlaceOccupeeDetect.Length; i++)
            {
                int couleur = LirePixel(
                    PlaceOccupeeDetect[i].X,
                    PlaceOccupeeDetect[i].Y);

                if (couleur == PlaceOccupeeDetect[i].Couleur)
                {
                    Clic(
                        PlaceOccupeeDetect[i].X,
                        PlaceOccupeeDetect[i].Y);

                    return true;
                }
            }

            return false;
        }

        // =========================================================
        // 🔥 PLUS DE JETONS
        // =========================================================
        private void Charger_Plus_De_Jetons_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Plus_De_Jetons.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            PlusDeJetonsDetect = JsonConvert.DeserializeObject<S_PlusDeJetonsDetect[]>(json);
        }
        private bool Extraire_Plus_De_Jetons(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bPlus_De_Jetons = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Plus_De_Jetons")
                {
                    bPlus_De_Jetons = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bPlus_De_Jetons)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    PlusDeJetonsDetect[index].Nom = nomCourant;
                    PlusDeJetonsDetect[index].X = int.Parse(t[1]);
                    PlusDeJetonsDetect[index].Y = int.Parse(t[2]);
                    PlusDeJetonsDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 1)
                return false;

            return true;
        }
        private void Sauver_Plus_De_Jetons()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Plus_De_Jetons.json");
            string json = JsonConvert.SerializeObject(PlusDeJetonsDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }
        private bool Plus_De_Jetons()
        {
            for (int i = 0; i < PlusDeJetonsDetect.Length; i++)
            {
                int couleur = LirePixel(
                    PlusDeJetonsDetect[i].X,
                    PlusDeJetonsDetect[i].Y);

                if (couleur == PlusDeJetonsDetect[i].Couleur)
                {
                    return true;
                }
            }

            return false;
        }
        private void Bt_Se_Recave_Click(object sender, RoutedEventArgs e)
        {
            if (Bt_Se_Recave.Background.Equals((Brush)FindResource("blanc")))
            {
                Bt_Se_Recave.Background = (Brush)FindResource("bleu");
                bSeRecaver = true;
            }
            else
            {
                Bt_Se_Recave.Background = (Brush)FindResource("blanc");
                bSeRecaver = false;
            }
        }
        // =========================================================
        // 🔥 PLUS DE POT
        // =========================================================
        private void Charger_Plus_De_Pot_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Plus_De_Pot.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            PlusDePotDetect =
                JsonConvert.DeserializeObject<S_PlusDePotDetect[]>(json);
        }
        private bool Extraire_Plus_De_Pot(string txt)
        {
            string[] lignes = txt.Split('\n');

            int index = 0;
            bool bPlus_De_Pot = false;
            string nomCourant = "";

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Plus_De_Pot")
                {
                    bPlus_De_Pot = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bPlus_De_Pot)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant =
                        ligne.Replace("COMMENT :", "").Trim();

                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =====================================
                    // SECURITE : TABLEAU PLEIN
                    // =====================================
                    if (index >= PlusDePotDetect.Length)
                        return false;

                    PlusDePotDetect[index].Nom = nomCourant;
                    PlusDePotDetect[index].X = int.Parse(t[1]);
                    PlusDePotDetect[index].Y = int.Parse(t[2]);
                    PlusDePotDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================
            if (index != 2)
                return false;

            return true;
        }
        private void Sauver_Plus_De_Pot()
        {
            Directory.CreateDirectory(DOSSIER_JSON);

            string fichier =
                Path.Combine(DOSSIER_JSON, "Plus_De_Pot.json");

            string json =
                JsonConvert.SerializeObject(
                    PlusDePotDetect,
                    Formatting.Indented);

            File.WriteAllText(fichier, json);
        }
        private bool Plus_De_Pot()
        {
            for (int i = 0; i < PlusDePotDetect.Length; i++)
            {
                int couleur = LirePixel(
                    PlusDePotDetect[i].X,
                    PlusDePotDetect[i].Y);

                // =====================================
                // UN DES DEUX PIXELS / COULEURS
                // =====================================
                if (couleur == PlusDePotDetect[i].Couleur)
                    return true;
            }

            return false;
        }// 🔥 PLUS DE POT ? L'UN OU L'AUTRE DES DEUX

        // =========================================================
        // 🔥 POSITION BOUTON
        // =========================================================
        private void Charger_Position_Bouton_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Position_Bouton.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            PositionBoutonDetect = JsonConvert.DeserializeObject<S_PositionBoutonDetect[]>(json);
        }
        private bool Extraire_Position_Bouton(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bPosition_Bouton = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Position_Bouton")
                {
                    bPosition_Bouton = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bPosition_Bouton)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    PositionBoutonDetect[index].Nom = nomCourant;
                    PositionBoutonDetect[index].X = int.Parse(t[1]);
                    PositionBoutonDetect[index].Y = int.Parse(t[2]);
                    PositionBoutonDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 9)
                return false;

            return true;
        }
        private void Sauver_Position_Bouton()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Position_Bouton.json");
            string json = JsonConvert.SerializeObject(PositionBoutonDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }
        private int Position_Bouton()
        {
            for (int i = 0; i < PositionBoutonDetect.Length; i++)
            {
                int couleur = LirePixel(
                    PositionBoutonDetect[i].X,
                    PositionBoutonDetect[i].Y);

                if (couleur == PositionBoutonDetect[i].Couleur)
                    return i + 1;   // Position du bouton : 1 à 9
            }

            return 0;               // Non détectée
        }

        // =========================================================
        // 🔥 PREFERENCES UTILISATEUR
        // =========================================================
        private void Charger_Preferences_Utilisateur_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Preferences_Utilisateur.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            PreferencesUtilisateur =
                JsonConvert.DeserializeObject<S_PreferencesUtilisateurDetect>(json);

            // =====================================
            // Recopie dans les variables de travail
            // =====================================
            bDebug = PreferencesUtilisateur.ModeDebug;
            DELAI_PRISE_CONTROLE = PreferencesUtilisateur.DelaiPriseControle;
            bSeRecaver = PreferencesUtilisateur.SeRecave;
            NIVEAU_MISE = PreferencesUtilisateur.NiveauMise;
            MEMO = PreferencesUtilisateur.Memo;
        }
        private bool Extraire_Preferences_Utilisateur(string txt)
        {
            bool modeDebugTrouve = false;
            bool delaiTrouve = false;
            bool changerPlaceTrouve = false;
            NbPreferences = 0;

            foreach (string ligneBrute in txt.Split('\n'))
            {
                string ligne = ligneBrute.Trim();

                if (ligne.StartsWith("COMMENT : \"ModeDebug\":"))
                {
                    string valeur = ligne.Replace("COMMENT : \"ModeDebug\":", "").Trim();

                    bDebug = bool.Parse(valeur);

                    modeDebugTrouve = true;
                    NbPreferences++;
                }

                if (ligne.StartsWith("COMMENT : \"DelaiPriseControle\":"))
                {
                    string valeur = ligne.Replace("COMMENT : \"DelaiPriseControle\":", "").Trim();

                    DELAI_PRISE_CONTROLE = int.Parse(valeur);

                    delaiTrouve = true;
                    NbPreferences++;
                }
                if (ligne.StartsWith("COMMENT : \"SeRecave\":"))
                {
                    string valeur = ligne.Replace("COMMENT : \"SeRecave\":", "").Trim();

                    bSeRecaver = bool.Parse(valeur);

                    NbPreferences++;
                }

                if (ligne.StartsWith("COMMENT : \"NiveauMise\":"))
                {
                    string valeur = ligne.Replace("COMMENT : \"NiveauMise\":", "").Trim();

                    NIVEAU_MISE = int.Parse(valeur);

                    NbPreferences++;
                }

                if (ligne.StartsWith("COMMENT : \"Memo\":"))
                {
                    string valeur = ligne.Replace("COMMENT : \"Memo\":", "").Trim();

                    MEMO = int.Parse(valeur);

                    NbPreferences++;
                }
            }
            return modeDebugTrouve || delaiTrouve || changerPlaceTrouve;
        }
        private void Sauver_Preferences_Utilisateur()
        {
            PreferencesUtilisateur.ModeDebug = bDebug;
            PreferencesUtilisateur.DelaiPriseControle = DELAI_PRISE_CONTROLE;
            PreferencesUtilisateur.SeRecave = bSeRecaver;
            PreferencesUtilisateur.NiveauMise = MiseSelectionnee; // PreferencesUtilisateur.NiveauMise = NIVEAU_MISE;
            PreferencesUtilisateur.Memo = MEMO;

            Directory.CreateDirectory(DOSSIER_JSON);

            string fichier = Path.Combine(DOSSIER_JSON, "Preferences_Utilisateur.json");

            string json = JsonConvert.SerializeObject(
                PreferencesUtilisateur,
                Formatting.Indented);

            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 PUPITRE GRIS
        // =========================================================
        private void Charger_Pupitre_Gris_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Pupitre_Gris.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            PupitreGrisDetect = JsonConvert.DeserializeObject<S_PupitreGrisDetect[]>(json);
        }
        private bool Extraire_Pupitre_Gris(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bPupitre_Gris = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Pupitre_Gris")
                {
                    bPupitre_Gris = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bPupitre_Gris)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    PupitreGrisDetect[index].Nom = nomCourant;
                    PupitreGrisDetect[index].X = int.Parse(t[1]);
                    PupitreGrisDetect[index].Y = int.Parse(t[2]);
                    PupitreGrisDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 2)
                return false;

            return true;
        }
        private void Sauver_Pupitre_Gris()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Pupitre_Gris.json");
            string json = JsonConvert.SerializeObject(PupitreGrisDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }
        private bool Pupitre_Gris()
        {
            for (int i = 0; i < PupitreGrisDetect.Length; i++)
            {
                int couleur = LirePixel(
                    PupitreGrisDetect[i].X,
                    PupitreGrisDetect[i].Y);

                if (couleur == PupitreGrisDetect[i].Couleur)
                    return true;
            }

            return false;
        }

        // =========================================================
        // 🔥 PUBICITE
        // =========================================================
        private void Charger_Publicite_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Publicite.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            PubliciteDetect = JsonConvert.DeserializeObject<S_PubliciteDetect[]>(json);
        }
        private bool Extraire_Publicite(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bPublicite = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Publicite")
                {
                    bPublicite = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bPublicite)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    PubliciteDetect[index].Nom = nomCourant;
                    PubliciteDetect[index].X = int.Parse(t[1]);
                    PubliciteDetect[index].Y = int.Parse(t[2]);
                    PubliciteDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 1)
                return false;

            return true;
        }
        private void Sauver_Publicite()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Publicite.json");
            string json = JsonConvert.SerializeObject(PubliciteDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }
        private async Task<bool> Fermer_Publicite()
        {
            for (int i = 0; i < PubliciteDetect.Length; i++)
            {
                int couleur = LirePixel(
                    PubliciteDetect[i].X,
                    PubliciteDetect[i].Y);

                if (couleur == PubliciteDetect[i].Couleur)
                {
                    Clic(
                        PubliciteDetect[i].X,
                        PubliciteDetect[i].Y);

                    // =====================================
                    // LAISSER LA FENÊTRE DISPARAÎTRE
                    // =====================================
                    await Task.Delay(150);

                    return true;
                }
            }

            return false;
        }

        // =========================================================
        // 🔥 RANGES
        // =========================================================

        // =====================================================
        // 🔥 RANGE TAPIS
        // =====================================================
        private void Charger_Range_Tapis_JSON()
        {
            string fichier =
                Path.Combine(
                    DOSSIER_JSON,
                    "Range_Tapis.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            var etat =
                System.Text.Json.JsonSerializer.Deserialize<EtatRange>(json);

            if (etat == null)
                return;

            MEMO = etat.Memo;

            RangeTapis.Clear();

            foreach (string main in etat.Range)
                RangeTapis.Add(main);
        }
        private void Sauver_Range_Tapis()
        {
            Directory.CreateDirectory(DOSSIER_JSON);

            string fichier =
                Path.Combine(
                    DOSSIER_JSON,
                    "Range_Tapis.json");

            var etat = new EtatRange
            {
                Memo = MEMO,
                Range = RangeTapis.ToList()
            };

            string json =
                System.Text.Json.JsonSerializer.Serialize(
                    etat,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                fichier,
                json);
        }

        // =====================================================
        // 🔥 RANGE RELANCE
        // =====================================================
        private void Charger_Range_Relance_JSON()
        {
            string fichier =
                Path.Combine(
                    DOSSIER_JSON,
                    "Range_Relance.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            var etat =
                System.Text.Json.JsonSerializer.Deserialize<EtatRange>(json);

            if (etat == null)
                return;

            RangeRelance.Clear();

            foreach (string main in etat.Range)
                RangeRelance.Add(main);
        }
        private void Sauver_Range_Relance()
        {
            Directory.CreateDirectory(DOSSIER_JSON);

            string fichier =
                Path.Combine(
                    DOSSIER_JSON,
                    "Range_Relance.json");

            var etat = new EtatRange
            {
                Memo = MEMO,
                Range = RangeRelance.ToList()
            };

            string json =
                System.Text.Json.JsonSerializer.Serialize(
                    etat,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                fichier,
                json);
        }

        // =====================================================
        // 🔥 RANGE Suivre
        // =====================================================
        private void Charger_Range_Suivre_JSON()
        {
            string fichier =
                Path.Combine(
                    DOSSIER_JSON,
                    "Range_Suivre.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            var etat =
                System.Text.Json.JsonSerializer.Deserialize<EtatRange>(json);

            if (etat == null)
                return;

            RangeSuivre.Clear();

            foreach (string main in etat.Range)
                RangeSuivre.Add(main);
        }
        private void Sauver_Range_Suivre()
        {
            Directory.CreateDirectory(DOSSIER_JSON);

            string fichier =
                Path.Combine(
                    DOSSIER_JSON,
                    "Range_Suivre.json");

            var etat = new EtatRange
            {
                Memo = MEMO,
                Range = RangeSuivre.ToList()
            };

            string json =
                System.Text.Json.JsonSerializer.Serialize(
                    etat,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                fichier,
                json);
        }

        // =========================================================
        // 🔥 RANGE ACTIF
        // =========================================================
        private enum TypeRange
        {
            Tapis,
            Relance,
            Suivre
        }

        private TypeRange RangeActif = TypeRange.Tapis;

        // =========================================================
        // 🔥 SÉLECTION RANGE EXCLUSIVE
        // =========================================================
        private void ActiverRange(Button boutonActif)
        {
            // =====================================
            // RESET DES 3 BOUTONS
            // =====================================
            Bt_Range_Tapis.Background = Brushes.White;
            Bt_Range_Relance.Background = Brushes.White;
            Bt_Range_Suivre.Background = Brushes.White;

            // =====================================
            // ACTIVATION
            // =====================================
            boutonActif.Background = Brushes.Orange;

            // =====================================
            // 🔥 RANGE LOGIQUE ACTIF
            // =====================================
            if (boutonActif == Bt_Range_Tapis)
            {
                RangeActif = TypeRange.Tapis;
            }
            else if (boutonActif == Bt_Range_Relance)
            {
                RangeActif = TypeRange.Relance;
            }
            else if (boutonActif == Bt_Range_Suivre)
            {
                RangeActif = TypeRange.Suivre;
            }

            // =====================================
            // RAFRAÎCHIR L'AFFICHAGE
            // =====================================
            RafraichirAffichage();
        }

        // =========================================================
        // 🔥 SUIS JE ASSIS
        // =========================================================
        private void Charger_Suis_Je_Assis_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Suis_Je_Assis.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            SuisJeAssisDetect = JsonConvert.DeserializeObject<S_SuisJeAssisDetect[]>(json);
        }
        private bool Extraire_Suis_Je_Assis(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bSuis_Je_Assis = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Suis_Je_Assis")
                {
                    bSuis_Je_Assis = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bSuis_Je_Assis)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    SuisJeAssisDetect[index].Nom = nomCourant;
                    SuisJeAssisDetect[index].X = int.Parse(t[1]);
                    SuisJeAssisDetect[index].Y = int.Parse(t[2]);
                    SuisJeAssisDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 18)
                return false;

            return true;
        }
        private void Sauver_Suis_Je_Assis()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Suis_Je_Assis.json");
            string json = JsonConvert.SerializeObject(SuisJeAssisDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }
        private int Suis_Je_Assis()
        {
            for (int i = 0; i < 18; i += 2)
            {
                int c1 = LirePixel(
                    SuisJeAssisDetect[i].X,
                    SuisJeAssisDetect[i].Y);

                int c2 = LirePixel(
                    SuisJeAssisDetect[i + 1].X,
                    SuisJeAssisDetect[i + 1].Y);

                // =====================================
                // UN OU L'AUTRE DES DEUX PIXELS
                // =====================================
                if (c1 == SuisJeAssisDetect[i].Couleur ||
                    c2 == SuisJeAssisDetect[i + 1].Couleur)
                {
                    return i / 2 + 1;
                }
            }

            return 0;
        }
        private void Lire_Ma_Position()
        {
            ResetPosition();

            MaPlace = Suis_Je_Assis();

            // Toujours mettre à jour l'avatar
            AfficherMonAvatar(MaPlace);

            if (MaPlace >= 1 && MaPlace <= 9)
            {
                ActiverPosition(BoutonsPosition[MaPlace - 1]);
            }
        }
        private void AfficherMonAvatar(int position)
        {
            Image[] avatars =
            {
                Mon_Avatar_1,
                Mon_Avatar_2,
                Mon_Avatar_3,
                Mon_Avatar_4,
                Mon_Avatar_5,
                Mon_Avatar_6,
                Mon_Avatar_7,
                Mon_Avatar_8,
                Mon_Avatar_9
            };

            // Masquer tous les avatars
            for (int i = 0; i < avatars.Length; i++)
                avatars[i].Visibility = Visibility.Hidden;

            // Afficher uniquement celui de la position
            if (position >= 1 && position <= 9)
                avatars[position - 1].Visibility = Visibility.Visible;
        }

        // =========================================================
        // 🔥 TABLE PRETE
        // =========================================================
        private void Charger_Table_Prete_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Table_Prete.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            TablePreteDetect = JsonConvert.DeserializeObject<S_TablePreteDetect[]>(json);
        }
        private bool Extraire_Table_Prete(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bTable_Prete = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Table_Prete")
                {
                    bTable_Prete = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bTable_Prete)
                    continue;

                // =====================================
                // COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant = ligne.Replace("COMMENT :", "").Trim();
                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith("IF PIXEL COLOR EQUALS"))
                {
                    string[] t = ligne.Split(':');
                    if (t.Length < 5)
                        continue;

                    TablePreteDetect[index].Nom = nomCourant;
                    TablePreteDetect[index].X = int.Parse(t[1]);
                    TablePreteDetect[index].Y = int.Parse(t[2]);
                    TablePreteDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================

            if (index != 1)
                return false;

            return true;
        }
        private void Sauver_Table_Prete()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Table_Prete.json");
            string json = JsonConvert.SerializeObject(TablePreteDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }
        private bool Table_Prete()
        {
            for (int i = 0; i < TablePreteDetect.Length; i++)
            {
                int couleur = LirePixel(
                TablePreteDetect[i].X,
                TablePreteDetect[i].Y);


            if (couleur == TablePreteDetect[i].Couleur)
                            return true;
            }

            return false;


        }


        // =========================================================
        // 🔥 TABLE STABILISÉE
        // =========================================================
        private void Charger_Table_Stabilisee_JSON()
        {
            string fichier =
                Path.Combine(
                    DOSSIER_JSON,
                    "Table_Stabilisee.json");

            if (!File.Exists(fichier))
                return;

            string json =
                File.ReadAllText(fichier);

            TableStabiliseeDetect =
                JsonConvert.DeserializeObject<
                    S_TableStabiliseeDetect[]>(json);
        }
        private bool Extraire_Table_Stabilisee(string txt)
        {
            string[] lignes =
                txt.Split('\n');

            int index = 0;

            bool bTable_Stabilisee = false;

            string nomCourant = "";

            // =====================================
            // 🔥 TABLEAU DES DÉTECTEURS
            // =====================================
            TableStabiliseeDetect =
                new S_TableStabiliseeDetect[2];

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne =
                    lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Table_Stabilisee")
                {
                    bTable_Stabilisee = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bTable_Stabilisee)
                    continue;

                // =====================================
                // NOUVEAU COMMENT :
                // =====================================
                if (ligne.StartsWith("COMMENT :"))
                {
                    nomCourant =
                        ligne
                        .Replace("COMMENT :", "")
                        .Trim();

                    continue;
                }

                // =====================================
                // IF PIXEL COLOR EQUALS
                // =====================================
                if (ligne.StartsWith(
                    "IF PIXEL COLOR EQUALS"))
                {
                    string[] t =
                        ligne.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =================================
                    // MAXIMUM 2 PIXELS
                    // =================================
                    if (index >= 2)
                        continue;

                    TableStabiliseeDetect[index].Nom =
                        nomCourant;

                    TableStabiliseeDetect[index].X =
                        int.Parse(t[1]);

                    TableStabiliseeDetect[index].Y =
                        int.Parse(t[2]);

                    TableStabiliseeDetect[index].Couleur =
                        int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNÉES NON CONFORMES
            // =====================================
            if (index != 2)
            {
                TableStabiliseeDetect =
                    Array.Empty<S_TableStabiliseeDetect>();

                return false;
            }

            return true;
        }
        private void Sauver_Table_Stabilisee()
        {
            Directory.CreateDirectory(
                DOSSIER_JSON);

            string fichier =
                Path.Combine(
                    DOSSIER_JSON,
                    "Table_Stabilisee.json");

            string json =
                JsonConvert.SerializeObject(
                    TableStabiliseeDetect,
                    Formatting.Indented);

            File.WriteAllText(
                fichier,
                json);
        }
        private bool Table_Stabilisee()
        {
            if (TableStabiliseeDetect == null ||
                TableStabiliseeDetect.Length == 0)
            {
                return false;
            }

            for (
                int i = 0;
                i < TableStabiliseeDetect.Length;
                i++)
            {
                int couleur =
                    LirePixel(
                        TableStabiliseeDetect[i].X,
                        TableStabiliseeDetect[i].Y);

                // =====================================
                // 🟢 UN SEUL PIXEL VERT SUFFIT
                // =====================================
                if (couleur ==
                    TableStabiliseeDetect[i].Couleur)
                {
                    return true;
                }
            }

            // =====================================
            // 🔴 AUCUN DES DEUX PIXELS N'EST VERT
            // =====================================
            return false;
        }
        #endregion

        #region DECISION
        // =========================================================
        // 🔥 DECISION
        // =========================================================

        // =========================================================
        // 🔥 BASCULER AUTO / MANUEL
        // =========================================================
        private void BasculerAutoManuel()
        {
            bModeManuel = !bModeManuel;

            if (bModeManuel)
            {
                Bt_Manuel.Background = Brushes.Red;
                Bt_Manuel.Content = "Manuel";
            }
            else
            {
                Bt_Manuel.Background = Brushes.LightGreen;
                Bt_Manuel.Content = "Auto";
            }
        }

        // =========================================================
        // BOUTON MANUEL
        // =========================================================
        private void Bt_Manuel_Click(
            object sender,
            RoutedEventArgs e)
        {
            BasculerAutoManuel();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;

                BasculerAutoManuel();
            }
        }
        private string Decision_Tapis_PreFlop(string carte1, string carte2, bool Je_Peux_Parler)
        {
            if (Main_Dans_Range_Tapis(carte1, carte2))
                return "Tapis";

            if (Je_Peux_Parler)
                return "Parole";

            return "Passe";
        }
        private bool Main_Dans_Range_Tapis(string carte1, string carte2)
        {
            string main = NormaliserMain(carte1, carte2);

            if (string.IsNullOrEmpty(main))
                return false;

            return EstDansRange(main);
        }

        // =========================================================
        // REPOS SOURIS
        // =========================================================
        private void Repos_Souris()
        {
            Clic(1100, 850);
        }
        #endregion

        #region ETATS        
        // =========================================================
        // RESET ETATS POKER
        // =========================================================
        private void Reset_Etats_Poker()
        {
            bMain1_OK = false;
            bMain2_OK = false;

            bFlop1_OK = false;
            bFlop2_OK = false;
            bFlop3_OK = false;

            bTurn_OK = false;

            bRiver_OK = false;
        }

        // =========================================================
        // ETAT LECTURE
        // =========================================================
        private bool bMain1_OK = false;
        private bool bMain2_OK = false;
        private bool bFlop1_OK = false;
        private bool bFlop2_OK = false;
        private bool bFlop3_OK = false;
        private bool bTurn_OK = false;
        private bool bRiver_OK = false;

        #endregion

        #region GESTION DES POINTS

        // =========================================================
        // 🔥 BOUTON POINTS
        // =========================================================
        private void Bt_Points_Click(object sender, RoutedEventArgs e)
        {
            bPoints = !bPoints;

            if (bPoints)
            {
                Bt_Points.Background = Brushes.LightGreen;

                for (int i = 0; i < 104; i++)
                {
                    Turn_HBDetect[i].Nom = Ajouter_Point(Turn_HBDetect[i].Nom);
                    River_HBDetect[i].Nom = Ajouter_Point(River_HBDetect[i].Nom);
                }
            }
            else
            {
                Bt_Points.Background = Brushes.White;

                for (int i = 0; i < 104; i++)
                {
                    Turn_HBDetect[i].Nom = Enlever_Point(Turn_HBDetect[i].Nom);
                    River_HBDetect[i].Nom = Enlever_Point(River_HBDetect[i].Nom);
                }
            }
        }

        // =========================================================
        // 🔥 AJOUTER POINT
        // =========================================================
        private string Ajouter_Point(string nom)
        {
            if (nom.EndsWith("."))
                return nom;

            return nom + ".";
        }

        // =========================================================
        // 🔥 ENLEVER POINT
        // =========================================================
        private string Enlever_Point(string nom)
        {
            return nom.TrimEnd('.');
        }

        #endregion

        #region HISTORIQUE
        // =========================================================
        // AJOUTER CARTE
        // =========================================================
        private void AjouterCarte(Paragraph p, TextBox tb, string separateur)
        {
            Run r = new Run(tb.Text + separateur);
            r.Foreground = tb.Foreground;
            p.Inlines.Add(r);
        }

        // =========================================================
        // 🔥 AJOUTER UNE MAIN DANS LE RICHTEXTBOX
        // =========================================================
        private void Ajouter_Main_RTB()
        {
            // =====================================
            // Ne pas afficher une main sans flop
            // =====================================
            if (TB_F1.Text == "")
                return;

            Paragraph p = new Paragraph();
            p.Margin = new Thickness(0);

            // =====================================
            // Colonnes
            // =====================================

            string main =
                TB_M1.Text + " " +
                TB_M2.Text;

            string flop =
                TB_F1.Text + " " +
                TB_F2.Text + " " +
                TB_F3.Text;

            string turnRiver =
                TB_T.Text + " " +
                TB_R.Text;

            // =====================================
            // Ligne complète
            // =====================================

            string ligne = "  " + main.PadRight(9) + "  " + flop.PadRight(14) + "  " + turnRiver;
            Run r = new Run(ligne);
            r.FontFamily = new FontFamily("Consolas");
            r.Foreground = Brushes.Black;
            p.Inlines.Add(r);
            RTB_Historique.Document.Blocks.Add(p);
        }

        // =========================================================
        // 🔥 SAUVEGARDER HISTORIQUE TXT
        // =========================================================
        private void Sauvegarder_Historique_TXT()
        {
            TextRange tr = new TextRange(
                RTB_Historique.Document.ContentStart,
                RTB_Historique.Document.ContentEnd);

            string texte = tr.Text;

            texte = texte.Replace(".", "");

            File.WriteAllText(FichierHistoriqueTXT, texte);
        }

        // =========================================================
        // 🔥 SAUVEGARDER HISTORIQUE RTF
        // =========================================================
        private void Sauvegarder_Historique_RTF()
        {
            FlowDocument doc = new FlowDocument();

            // =====================================
            // Charger l'ancien RTF
            // =====================================

            if (File.Exists(FichierHistoriqueRTF))
            {
                FileInfo fi = new FileInfo(FichierHistoriqueRTF);

                if (fi.Length > 0)
                {
                    TextRange tr =
                        new TextRange(
                            doc.ContentStart,
                            doc.ContentEnd);

                    using (FileStream fs = new FileStream(
                        FichierHistoriqueRTF,
                        FileMode.Open,
                        FileAccess.Read))
                    {
                        tr.Load(
                            fs,
                            DataFormats.Rtf);
                    }
                }
            }

            // =====================================
            // Ajouter les Paragraph du RTB
            // =====================================

            foreach (Block b in RTB_Historique.Document.Blocks)
            {
                if (b is Paragraph pSource)
                {
                    Paragraph p = new Paragraph();

                    p.Margin = pSource.Margin;
                    p.LineHeight = pSource.LineHeight;
                    p.TextAlignment = pSource.TextAlignment;

                    foreach (Inline inline in pSource.Inlines)
                    {
                        if (inline is Run r)
                        {
                            Run nouveau = new Run();

                            foreach (char c in r.Text)
                            {
                                if (c != '.')
                                    nouveau.Text += c;
                            }

                            nouveau.Foreground = r.Foreground;
                            nouveau.FontFamily = r.FontFamily;
                            nouveau.FontSize = r.FontSize;
                            nouveau.FontWeight = r.FontWeight;
                            nouveau.FontStyle = r.FontStyle;

                            p.Inlines.Add(nouveau);
                        }
                    }

                    doc.Blocks.Add(p);
                }
            }

            // =====================================
            // Sauvegarder
            // =====================================

            TextRange trRTF =
                new TextRange(
                    doc.ContentStart,
                    doc.ContentEnd);

            using (FileStream fs = new FileStream(
                FichierHistoriqueRTF,
                FileMode.Create))
            {
                trRTF.Save(
                    fs,
                    DataFormats.Rtf);
            }

            // =====================================
            // Vider le RTB
            // =====================================

            RTB_Historique.Document.Blocks.Clear();
        }

        // =========================================================
        // 🔥 SAUVEGARDER HISTORIQUE JSON
        // =========================================================
        private void Sauvegarder_Historique_JSON()
        {
            List<MainHistorique> liste;

            // =====================================
            // Charger l'ancien JSON
            // =====================================

            if (File.Exists(FichierHistoriqueJSON))
            {
                string ancienJson =
                    File.ReadAllText(
                        FichierHistoriqueJSON);

                if (ancienJson.Trim() != "")
                {
                    liste =
                        System.Text.Json.JsonSerializer.Deserialize<
                            List<MainHistorique>>(
                                ancienJson);

                    if (liste == null)
                    {
                        liste =
                            new List<MainHistorique>();
                    }
                }
                else
                {
                    liste =
                        new List<MainHistorique>();
                }
            }
            else
            {
                liste =
                    new List<MainHistorique>();
            }

            // =====================================
            // Ajouter les nouvelles mains
            // =====================================

            string[] lignes =
                File.ReadAllLines(
                    FichierHistoriqueTXT);

            foreach (string ligne in lignes)
            {
                string[] cartes =
                    ligne.Split(
                        new char[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries);

                MainHistorique h =
                    new MainHistorique();

                if (cartes.Length > 0)
                    h.M1 = cartes[0];

                if (cartes.Length > 1)
                    h.M2 = cartes[1];

                if (cartes.Length > 2)
                    h.F1 = cartes[2];

                if (cartes.Length > 3)
                    h.F2 = cartes[3];

                if (cartes.Length > 4)
                    h.F3 = cartes[4];

                if (cartes.Length > 5)
                    h.T = cartes[5];

                if (cartes.Length > 6)
                    h.R = cartes[6];

                liste.Add(h);
            }

            // =====================================
            // Sauvegarder le JSON complet
            // =====================================

            string json =
                System.Text.Json.JsonSerializer.Serialize(
                    liste,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder =
                            System.Text.Encodings.Web
                            .JavaScriptEncoder
                            .UnsafeRelaxedJsonEscaping
                    });

            File.WriteAllText(
                FichierHistoriqueJSON,
                json);
        }

        // =========================================================
        // 🔥 VIDER HISTORIQUES
        // =========================================================
        private void Vider_Historiques()
        {
            File.WriteAllText(FichierHistoriqueTXT, "");
            File.WriteAllText(FichierHistoriqueRTF, "");
            File.WriteAllText(FichierHistoriqueJSON, "");

            RTB_Historique.Document.Blocks.Clear();
        }
        // =========================================================
        // AFFICHAGE HISTORIQUE
        // =========================================================
        private void Affiche_Mise(string texte)
        {
            Paragraph p = new Paragraph();

            p.Margin = new Thickness(0);
            p.LineHeight = 1;

            p.Inlines.Add(new Run(texte));

            RTB_Historique.Document.Blocks.Add(p);

            RTB_Historique.ScrollToEnd();
        }

        // =========================================================
        // AJOUTER HISTORIQUE
        // =========================================================
        private void Ajouter_Historique()
        {

            if (!bAutoriserHistorique || TB_M1.Text == "")
                return;

            Paragraph p = new Paragraph();

            p.Margin = new Thickness(0);
            p.LineHeight = 1;

            // =====================================
            // Longueur Main
            // =====================================

            int lgMain = TB_M1.Text.Length + 1 + TB_M2.Text.Length;

            int nbEspacesMainFlop = 9 - lgMain;

            if (nbEspacesMainFlop < 2)
                nbEspacesMainFlop = 2;

            // =====================================
            // Longueur Flop
            // =====================================

            int lgFlop =
                TB_F1.Text.Length +
                1 +
                TB_F2.Text.Length +
                1 +
                TB_F3.Text.Length;

            int nbEspacesFlopTurn = 14 - lgFlop;

            if (nbEspacesFlopTurn < 2)
                nbEspacesFlopTurn = 2;

            // =====================================
            // Main
            // =====================================

            AjouterCarte(p, TB_M1, " ");
            AjouterCarte(p, TB_M2, new string(' ', nbEspacesMainFlop));

            // =====================================
            // Flop
            // =====================================

            AjouterCarte(p, TB_F1, " ");
            AjouterCarte(p, TB_F2, " ");
            AjouterCarte(
                p,
                TB_F3,
                new string(' ', nbEspacesFlopTurn));

            // =====================================
            // Turn River
            // =====================================

            AjouterCarte(p, TB_T, " ");
            AjouterCarte(p, TB_R, "");

            //MessageBox.Show("Autorise = " + bAutoriserHistorique + "\nM1 = " + TB_M1.Text + "\nBlocks avant = " + RTB_Historique.Document.Blocks.Count);

            RTB_Historique.Document.Blocks.Add(p);

            //MessageBox.Show("Blocks après = " + RTB_Historique.Document.Blocks.Count);

            RTB_Historique.ScrollToEnd();
        }

        #endregion

        #region INJECTION

        // =========================================================
        // 🔥 BOUTON INJECTION
        // =========================================================
        private void Bt_Injection_Click(object sender, RoutedEventArgs e)
        {
            // =====================================
            // LECTURE PRESSE-PAPIERS
            // =====================================
            string txt = Clipboard.GetText();

            // =====================================
            // RIEN ?
            // =====================================
            if (string.IsNullOrWhiteSpace(txt))
            {
                MessageBox.Show("Aucune donnée exploitable");
                return;
            }

            // =====================================
            // AFFICHAGE BRUT INJECTION
            // =====================================
            TB_Injection.Text = txt;

            // =====================================
            // EXTRACTION
            // =====================================
            bool okMain1                    = Extraire_Main1(txt);
            bool okMain2                    = Extraire_Main2(txt);
            bool okFlop1                    = Extraire_Flop1(txt);
            bool okFlop2                    = Extraire_Flop2(txt);
            bool okFlop3                    = Extraire_Flop3(txt);
            bool okTurn                     = Extraire_Turn(txt);
            bool okTurn_HB                  = Extraire_Turn_HB(txt);
            bool okRiver                    = Extraire_River(txt);
            bool okRiver_HB                 = Extraire_River_HB(txt);

            bool okAmitie                   = Extraire_Amitie(txt);
            bool okAucune_Table             = Extraire_Aucune_Table(txt);
            bool okCartographie_Du_Pot      = Extraire_Cartographie_Du_Pot(txt);
            bool okDecision                 = Extraire_Decision(txt);
            bool okFelicitation             = Extraire_Felicitation(txt);
            bool okInfo_Joueur              = Extraire_Info_Joueur(txt);
            bool okJe_Ne_Suis_Pas_Assis     = Extraire_Je_Ne_Suis_Pas_Assis(txt);

            bool okJoueur_A_Tapis           = Extraire_Joueur_A_Tapis(txt);
            bool okJoueur_Parole            = Extraire_Joueur_Parole(txt);
            bool okJoueur_Reflechit         = Extraire_Joueur_Reflechit(txt);
            bool okJoueur_Relance           = Extraire_Joueur_Relance(txt);
            bool okJoueur_Se_Couche         = Extraire_Joueur_Se_Couche(txt);
            bool okJoueur_Suit              = Extraire_Joueur_Suit(txt);

            bool okGagne                    = Extraire_Gagne(txt);
            bool okMain_Board               = Extraire_Main_Board(txt);
            bool okNiveau_Table             = Extraire_Niveau_Table(txt);
            bool okNouveau_Niveau           = Extraire_Nouveau_Niveau(txt);
            bool okPlaces_Libres            = Extraire_Places_Libres(txt);
            bool okPlace_Occupee            = Extraire_Place_Occupee(txt);
            bool okPosition_Bouton          = Extraire_Position_Bouton(txt);
            bool okPlus_De_Jetons           = Extraire_Plus_De_Jetons(txt);
            bool okPlus_De_Pot              = Extraire_Plus_De_Pot(txt);
            bool okPreferences_Utilisateur  = Extraire_Preferences_Utilisateur(txt);
            bool okPublicite                = Extraire_Publicite(txt);
            bool okPupitre_Gris             = Extraire_Pupitre_Gris(txt);
            bool okSuis_Je_Assis            = Extraire_Suis_Je_Assis(txt);
            bool okTable_Prete              = Extraire_Table_Prete(txt);
            bool okTable_Stabilisee         = Extraire_Table_Stabilisee(txt);

            // =====================================
            // AUCUNE DONNÉE RECONNUE
            // =====================================
            if (!okMain1                    &&
                !okMain2                    &&
                !okFlop1                    &&
                !okFlop2                    &&
                !okFlop3                    &&
                !okTurn                     &&
                !okTurn_HB                  &&
                !okRiver                    &&
                !okRiver_HB                 &&
                !okAmitie                   &&
                !okAucune_Table             &&
                !okCartographie_Du_Pot      &&
                !okDecision                 &&
                !okMain_Board               &&
                !okFelicitation             &&
                !okInfo_Joueur              &&
                !okJe_Ne_Suis_Pas_Assis     &&
                !okNiveau_Table             &&
                !okNouveau_Niveau           &&
                !okJoueur_A_Tapis           &&
                !okJoueur_Parole            &&
                !okJoueur_Reflechit         &&
                !okJoueur_Relance           &&
                !okJoueur_Se_Couche         &&
                !okJoueur_Suit              &&
                !okGagne                    &&
                !okPlaces_Libres            &&
                !okPlace_Occupee            &&
                !okPosition_Bouton          &&
                !okPlus_De_Jetons           &&
                !okPlus_De_Pot              &&
                !okPreferences_Utilisateur  &&
                !okPublicite                &&
                !okPupitre_Gris             &&
                !okSuis_Je_Assis            &&
                !okTable_Prete              &&
                !okTable_Stabilisee)
            {
                MessageBox.Show("Aucune donnée reconnue");
                return;
            }

            // =====================================
            // SAUVEGARDE JSON
            // =====================================
            if (okMain1)
                Sauver_Main1();

            if (okMain2)
                Sauver_Main2();

            if (okFlop1)
                Sauver_Flop1();

            if (okFlop2)
                Sauver_Flop2();

            if (okFlop3)
                Sauver_Flop3();

            if (okTurn)
                Sauver_Turn();

            if (okTurn_HB)
                Sauver_Turn_HB();

            if (okRiver)
                Sauver_River();

            if (okRiver_HB)
                Sauver_River_HB();

            if (okAmitie)
                Sauver_Amitie();

            if (okAucune_Table)
                Sauver_Aucune_Table();

            if (okCartographie_Du_Pot)
                Sauver_Cartographie_Du_Pot();

            if (okDecision)
                Sauver_Decision();

            if (okFelicitation)
                Sauver_Felicitation();

            if (okInfo_Joueur)
                Sauver_Info_Joueur();

            if (okJe_Ne_Suis_Pas_Assis)
                Sauver_Je_Ne_Suis_Pas_Assis();

            if (okJoueur_A_Tapis)
                Sauver_Joueur_A_Tapis();

            if (okJoueur_Parole)
                Sauver_Joueur_Parole();

            if (okJoueur_Reflechit)
                Sauver_Joueur_Reflechit();

            if (okJoueur_Relance)
                Sauver_Joueur_Relance();

            if (okJoueur_Se_Couche)
                Sauver_Joueur_Se_Couche();

            if (okJoueur_Suit)
                Sauver_Joueur_Suit();

            if (okGagne)
                Sauver_Gagne();

            if (okMain_Board)
                Sauver_Main_Board();

            if (okNiveau_Table)
                Sauver_Niveau_Table();

            if (okNouveau_Niveau)
                Sauver_Nouveau_Niveau();

            if (okPlaces_Libres)
                Sauver_Places_Libres();

            if (okPlace_Occupee)
                Sauver_Place_Occupee();

            if (okPosition_Bouton)
                Sauver_Position_Bouton();

            if (okPlus_De_Jetons)
                Sauver_Plus_De_Jetons();

            if (okPlus_De_Pot)
                Sauver_Plus_De_Pot();

            if (okPreferences_Utilisateur)
                Sauver_Preferences_Utilisateur();

            if (okPublicite)
                Sauver_Publicite();

            if (okPupitre_Gris)
                Sauver_Pupitre_Gris();

            if (okSuis_Je_Assis)
                Sauver_Suis_Je_Assis();

            if (okTable_Prete)
                Sauver_Table_Prete();

            if (okTable_Stabilisee)
                Sauver_Table_Stabilisee();

            // =====================================
            // RESULTAT
            // =====================================
            string msg =
                "Injection JSON OK\n\n";

            if (okMain1)
                msg += "Main_1 : 52 cartes\n";

            if (okMain2)
                msg += "Main_2 : 52 cartes\n";

            if (okFlop1)
                msg += "Flop_1 : 52 cartes\n";

            if (okFlop2)
                msg += "Flop_2 : 52 cartes\n";

            if (okFlop3)
                msg += "Flop_3 : 52 cartes\n";

            if (okTurn)
                msg += "Turn : 52 cartes\n";

            if (okTurn_HB)
                msg += "Turn_HB : 104 cartes\n";

            if (okRiver)
                msg += "River : 52 cartes\n";

            if (okRiver_HB)
                msg += "River_HB : 104 cartes\n";

            if (okAmitie)
                msg += "Amitie : 1 détecteur\n";

            if (okAucune_Table)
                msg += "Aucune_Table : 1 détecteur\n";

            if (okCartographie_Du_Pot)
                msg += "Cartographie_Du_Pot : chargée\n";

            if (okDecision)
                msg += "Decision : 5 détecteurs\n";

            if (okFelicitation)
                msg += "Felicitation : 2 détecteurs\n";

            if (okInfo_Joueur)
                msg += "Info_Joueur : 1 détecteur\n";

            if (okJe_Ne_Suis_Pas_Assis)
                msg += "Je_Ne_Suis_Pas_Assis : 1 détecteur\n";

            if (okMain_Board)
                msg += "Main_Board : 82 détecteurs\n";

            if (okJoueur_A_Tapis)
                msg += "Joueur_A_Tapis : 9 détecteurs\n";

            if (okJoueur_Parole)
                msg += "Joueur_Parole : 9 détecteurs\n";

            if (okJoueur_Reflechit)
                msg += "Joueur_Reflechit : 9 détecteurs\n";

            if (okJoueur_Relance)
                msg += "Joueur_Relance : 9 détecteurs\n";

            if (okJoueur_Se_Couche)
                msg += "Joueur_Se_Couche : 9 détecteurs\n";

            if (okJoueur_Suit)
                msg += "Joueur_Suit : 9 détecteurs\n";

            if (okGagne)
                msg += "Gagne : 18 détecteurs\n";

            if (okNiveau_Table)
                msg += "Niveau_Table : 12 détecteurs\n";

            if (okNouveau_Niveau)
                msg += "Nouveau_Niveau : 1 détecteur\n";

            if (okPlaces_Libres)
                msg += "Places_Libres : 18 détecteurs\n";

            if (okPlace_Occupee)
                msg += "Place_Occupee : 1 détecteur\n";

            if (okPreferences_Utilisateur)
                msg += "Préférences_Utilisateur : " +
                       NbPreferences +
                       " Préférences\n";

            if (okPosition_Bouton)
                msg += "Position_Bouton : 9 détecteurs\n";

            if (okPlus_De_Jetons)
                msg += "Plus_De_Jetons : 1 détecteur\n";

            if (okPlus_De_Pot)
                msg += "Plus_De_Pot : 1 détecteur\n";

            if (okPublicite)
                msg += "Publicite : 1 détecteur\n";

            if (okPupitre_Gris)
                msg += "Pupitre_Gris : 2 détecteurs\n";

            if (okSuis_Je_Assis)
                msg += "Suis_Je_Assis : 18 détecteurs\n";

            if (okTable_Prete)
                msg += "Table_Prete : 1 détecteur\n";

            if (okTable_Stabilisee)
                msg += "Table_Stabilisee : 2 détecteurs\n";

            MessageBox.Show(msg);
        }


        #endregion

        #region MEMO
        // =========================================================
        // 🔥 MEMO
        // =========================================================
        private void ChargerRangeTapis(List<string> liste)
        {
            RangeTapis.Clear();

            foreach (var main in liste)
                RangeTapis.Add(main);

            RafraichirAffichage();
        }

        private void ActiverMemo(Button boutonActif)
        {
            foreach (var b in BoutonsMemo)
                b.Background = (Brush)FindResource("blanc");

            boutonActif.Background = (Brush)FindResource("orange");
        }
        // =========================================================
        // 🔥 CHARGER UN MEMO DANS LE RANGE ACTIF
        // =========================================================
        private void ChargerRangeSelonMode(List<string> liste)
        {
            switch (RangeActif)
            {
                case TypeRange.Tapis:

                    RangeTapis.Clear();

                    foreach (string main in liste)
                        RangeTapis.Add(main);

                    break;

                case TypeRange.Relance:

                    RangeRelance.Clear();

                    foreach (string main in liste)
                        RangeRelance.Add(main);

                    break;

                case TypeRange.Suivre:

                    RangeSuivre.Clear();

                    foreach (string main in liste)
                        RangeSuivre.Add(main);

                    break;
            }

            RafraichirAffichage();
        }
        private void Bt_Mem01_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 1;
            ActiverMemo(Bt_Mem01);
            ChargerRangeSelonMode(new List<string>
            {
                "AA","KK"
            });
        }
        private void Bt_Mem02_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 2;
            ActiverMemo(Bt_Mem02);
            ChargerRangeSelonMode(new List<string>
            {
                "AA",   "KK",   "QQ",
                "AKp"
            });
        }
        private void Bt_Mem03_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 3;
            ActiverMemo(Bt_Mem03);
            ChargerRangeSelonMode(new List<string>
            {
                "AA",   "KK",   "QQ",
                "AKp",  "AQp",
                "AK"
            });
        }
        private void Bt_Mem04_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 4;
            ActiverMemo(Bt_Mem04);
            ChargerRangeSelonMode(new List<string>
            {
                "AA",   "KK",   "QQ",   "JJ",   "1010", "99",   "88",
                "AKp",  "AQp",  "AJp",
                "AK",   "AQ"
            });
        }
        private void Bt_Mem05_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 5;
            ActiverMemo(Bt_Mem05);
            ChargerRangeSelonMode(new List<string>
            {
                "AA",   "KK",   "QQ",   "JJ",   "1010", "99",   "88",   "77",   "66",
                "AKp",  "AQp",  "AJp",  "A10p",
                "AK",   "AQ",   "AJ",
                "KQp"
            });
        }
        private void Bt_Mem06_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 6;
            ActiverMemo(Bt_Mem06);
            ChargerRangeSelonMode(new List<string>
            {
                "AA",   "KK",   "QQ",   "JJ",   "1010", "99",   "88",   "77",   "66",   "55",   "44",
                "AKp",  "AQp",  "AJp",  "A10p", "A9p",
                "AK",   "AQ",   "AJ",   "A10",
                "KQp",  "KJp",
                "KQ"
            });
        }
        private void Bt_Mem07_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 7;
            ActiverMemo(Bt_Mem07);
            ChargerRangeSelonMode(new List<string>
            {
                "AA",   "KK",   "QQ",   "JJ",   "1010", "99",   "88",   "77",   "66",   "55",   "44",   "33",   "22",
                "AKp",  "AQp",  "AJp",  "A10p", "A9p",  "A8p",
                "AK",   "AQ",   "AJ",   "A10",  "A9",
                "KQp",  "KJp",  "K10p",
                "KQ"
            });
        }
        private void Bt_Mem08_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 8;
            ActiverMemo(Bt_Mem08);
            ChargerRangeSelonMode(new List<string>
            {
                "AA",   "KK",   "QQ",   "JJ",   "1010", "99",   "88",   "77",   "66",   "55",   "44",   "33",   "22",
                "AKp",  "AQp",  "AJp",  "A10p", "A9p",  "A8p",  "A7p",
                "AK",   "AQ",   "AJ",   "A10",  "A9",   "A8",
                "KQp",  "KJp",  "K10p",
                "KQ",   "KJ"
            });
        }
        private void Bt_Mem09_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 9;
            ActiverMemo(Bt_Mem09);
            ChargerRangeSelonMode(new List<string>
            {
                "AA",   "KK",   "QQ",   "JJ",   "1010", "99",   "88",   "77",   "66",   "55",   "44",   "33",   "22",
                "AKp",  "AQp",  "AJp",  "A10p", "A9p",  "A8p",  "A7p",  "A6p",
                "AK",   "AQ",   "AJ",   "A10",  "A9",   "A8",   "A7",
                "KQp",  "KJp",  "K10p", "K9p",
                "KQ",   "KJ",   "K10"
            });
        }
        private void Bt_Mem10_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 10;
            ActiverMemo(Bt_Mem10);
            ChargerRangeSelonMode(new List<string>
            {
                "AA",   "KK",   "QQ",   "JJ",   "1010", "99",   "88",   "77",   "66",   "55",   "44",   "33",   "22",
                "AKp",  "AQp",  "AJp",  "A10p", "A9p",  "A8p",  "A7p",  "A6p",  "A5p",
                "AK",   "AQ",   "AJ",   "A10",  "A9",   "A8",   "A7",   "A6",
                "KQp",  "KJp",  "K10p", "K9p",  "K8p",
                "KQ",   "KJ",   "K10"
            });
        }

        private void Bt_Mem11_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 11;
            ActiverMemo(Bt_Mem11);
            ChargerRangeSelonMode(new List<string>
            {
                "AA",   "KK",   "QQ",   "JJ",   "1010", "99",   "88",   "77",   "66",   "55",   "44",   "33",   "22",
                "AKp",  "AQp",  "AJp",  "A10p", "A9p",  "A8p",  "A7p",  "A6p",  "A5p",  "A4p",  "A3p",  "A2p",
                "AK",   "AQ",   "AJ",   "A10",  "A9",   "A8",   "A7",   "A6",   "A5",
                "KQp",  "KJp",  "K10p", "K9p",  "K8p",  "K7p",
                "KQ",   "KJ",   "K10",  "K9"
            });
        }
        private void Bt_Mem12_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 12;
            ActiverMemo(Bt_Mem12);
            ChargerRangeSelonMode(new List<string>
            {
                "AA",   "KK",   "QQ",   "JJ",   "1010", "99",   "88",   "77",   "66",   "55",   "44",   "33",   "22",
                "AKp",  "AQp",  "AJp",  "A10p", "A9p",  "A8p",  "A7p",  "A6p",  "A5p",  "A4p",  "A3p",  "A2p",
                "AK",   "AQ",   "AJ",   "A10",  "A9",   "A8",   "A7",   "A6",   "A5",   "A4",
                "KQp",  "KJp",  "K10p", "K9p",  "K8p",  "K7p",  "K6p",
                "KQ",   "KJ",   "K10",  "K9",   "QJp"
            });
        }
        private void Bt_Mem13_Click(object sender, RoutedEventArgs e)
        {
            MEMO = 13;
            ActiverMemo(Bt_Mem13);
            ChargerRangeSelonMode(new List<string>
            {

            });
        }

        #endregion

        #region RESETS
        // =========================================================
        // RESET TABLE
        // =========================================================
        private void Reset_Table()
        {
            Reset_Board();
            Reset_Boutons();
            Reset_Calculs();
        }

        // =========================================================
        // RESET BOARD
        // =========================================================
        private void Reset_Board()
        {
            TB_M1.Clear();
            TB_M2.Clear();

            TB_F1.Clear();
            TB_F2.Clear();
            TB_F3.Clear();

            TB_T.Clear();
            TB_R.Clear();
        }
        // =========================================================
        // RESET BOUTONS
        // =========================================================
        private void Reset_Boutons()
        {
            Bt_Main_1.Background = Brushes.White;
            Bt_Main_2.Background = Brushes.White;

            Bt_Flop_1.Background = Brushes.White;
            Bt_Flop_2.Background = Brushes.White;
            Bt_Flop_3.Background = Brushes.White;

            Bt_Turn.Background  = Brushes.White;
            Bt_River.Background = Brushes.White;
        }

        //// =========================================================
        //// RESET CALCULS
        //// =========================================================
        private void Reset_Calculs()
        {
            Reset_PreFlop();

            Reset_Flop();
            Reset_Tirages_Flop();

            Reset_Turn();
            Reset_Tirages_Turn();

            Reset_River();
        }

        // =========================================================
        // RESET PreFlop Flop Turn River
        // =========================================================
        private void Reset_PreFlop()
        {
            Bt_PreFlop_Paire.Background = Blanc;
            Bt_PreFlop_Pareille.Background = Blanc;
            Bt_PreFlop_Suite.Background = Blanc;
        }
        private void Reset_Flop()
        {
            Bt_Flop_Paire.Background = Blanc;
            Bt_Flop_2_Paires.Background = Blanc;
            Bt_Flop_Brelan.Background = Blanc;
            Bt_Flop_Quinte.Background = Blanc;
            Bt_Flop_Couleur.Background = Blanc;
            Bt_Flop_Full.Background = Blanc;
            Bt_Flop_Carre.Background = Blanc;
            Bt_Flop_Quinte_Flush.Background = Blanc;
        }
        private void Reset_Tirages_Flop()
        {
            bTir_Flop_Quinte = false;
            bTir_Flop_Ventrale = false;
            bTir_Flop_Couleur = false;
            bTir_Flop_QFlush = false;

            Bt_Tir_Flop_Quinte.Content = "Q";
            Bt_Tir_Flop_Quinte.Background = Blanc;

            Bt_Tir_Flop_Couleur.Background = Blanc;
            Bt_Tir_Flop_Quinte_Flush.Background = Blanc;
        }
        private void Reset_Turn()
        {
            Bt_Turn_Paire.Background = Blanc;
            Bt_Turn_2_Paires.Background = Blanc;
            Bt_Turn_Brelan.Background = Blanc;
            Bt_Turn_Quinte.Background = Blanc;
            Bt_Turn_Couleur.Background = Blanc;
            Bt_Turn_Full.Background = Blanc;
            Bt_Turn_Carre.Background = Blanc;
            Bt_Turn_Quinte_Flush.Background = Blanc;
        }
        private void Reset_Tirages_Turn()
        {
            bTir_Turn_Quinte = false;
            bTir_Turn_Ventrale = false;
            bTir_Turn_Couleur = false;
            bTir_Turn_QFlush = false;

            Bt_Tir_Turn_Quinte.Content = "Q";
            Bt_Tir_Turn_Quinte.Background = Blanc;

            Bt_Tir_Turn_Couleur.Background = Blanc;
            Bt_Tir_Turn_Quinte_Flush.Background = Blanc;
        }
        private void Reset_River()
        {
            Bt_River_Paire.Background = Blanc;
            Bt_River_2_Paires.Background = Blanc;
            Bt_River_Brelan.Background = Blanc;
            Bt_River_Quinte.Background = Blanc;
            Bt_River_Couleur.Background = Blanc;
            Bt_River_Full.Background = Blanc;
            Bt_River_Carre.Background = Blanc;
            Bt_River_Quinte_Flush.Background = Blanc;
        }

        // =========================================================
        // RESET LECTURE BOARD
        // =========================================================
        private void Reset_Lecture_Board()
        {
            bMain1_Lu = false;
            bMain2_Lu = false;

            bFlop1_Lu = false;
            bFlop2_Lu = false;
            bFlop3_Lu = false;

            bTurn_Lu = false;
            bRiver_Lu = false;
        }

        #endregion

        #region SE RECAVER
        // =========================================================
        // SE RECAVER
        // =========================================================
        private void Se_Recaver() // PRIVATE : Visible uniquement ici.
        {
            switch (MiseSelectionnee)
            {
                case -9: Recave_M(9); break;
                case -8: Recave_M(8); break;
                case -6: Recave_M(6); break;
                case -4: Recave_M(4); break;
                case -2: Recave_M(2); break;

                case 0: break; // Aucun déplacement.

                case 2: Recave_P(2); break;
                case 4: Recave_P(4); break;
                case 6: Recave_P(6); break;
                case 8: Recave_P(8); break;
                case 10: Recave_P(10); break;
            }

            // Validation
            Clic(PlusDeJetonsDetect[0].X, PlusDeJetonsDetect[0].Y);
        }

        // =========================================================
        // RECAVE MOINS
        // =========================================================
        private void Recave_M(int Nombre)
        {
            for (int i = 0; i < Nombre; i++)
            {
                Clic(845, 560);
            }
        }

        // =========================================================
        // RECAVE PLUS
        // =========================================================
        private void Recave_P(int Nombre)
        {
            for (int i = 0; i < Nombre; i++)
            {
                Clic(1070, 560);
            }
        }

        // Délai entre l'appui et le relâchement du clic (ms)
        private int DelaiClic = 10;

        // =========================================================
        // CLIC
        // =========================================================
        private void Clic(int X, int Y)
        {
            SetCursorPos(X, Y);

            Thread.Sleep(DelaiClic);

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);

            Thread.Sleep(DelaiClic);

            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

            Thread.Sleep(DelaiClic);
        }

        #endregion

        #region TEST_BOARD TRIGGERS_BOARD TAPIS_PREFLOP IA

        // =========================================================
        // 🔥 TEST_BOARD
        // =========================================================
        private async void Bt_Test_Board_Click(object sender, RoutedEventArgs e)
        {
            // =====================================
            // STOP
            // =====================================
            bSuperviseurActif = false;
            if (bTestBoard_Actif)
            {
                StopTousModes();

                return;
            }

            // =====================================
            // START
            // =====================================
            StopTousModes();

            await Task.Delay(50);

            bool bGris = false;

            bTestBoard_Actif = true;
            // =====================================
            // BOUTONS
            // =====================================
            Reset_Boutons();

            Bt_Test_Board.Background = Brushes.LightGreen;

            try
            {
                bTestBoard_Actif = true;

                // =====================================
                // BOUCLE
                // =====================================

                while (bTestBoard_Actif)
                {
                    // =====================================
                    // Capture écran
                    // =====================================
                    CaptureEcran();

                    // ==========================
                    // PASSAGE AU GRIS
                    // ==========================

                    if (Pupitre_Gris())
                    {
                        bGris = true;
                    }

                    // ==========================
                    // RETOUR AU BLANC
                    // ==========================

                    if (bGris &&
                        !Pupitre_Gris())
                    {
                        bGris = false;

                        Reset_Board();

                        continue;
                    }

                    // =====================================
                    // LECTURE EVENEMENTIELLE
                    // =====================================

                    if (Detecter_Presence("Main_1_Presence"))
                    {
                        Lire_Main_1();
                    }

                    if (Detecter_Presence("Main_2_Presence"))
                    {
                        Lire_Main_2();
                    }

                    if (Detecter_Presence("Flop_1_Presence"))
                    {
                        Lire_Flop_1();
                    }

                    if (Detecter_Presence("Flop_2_Presence"))
                    {
                        Lire_Flop_2();
                    }

                    if (Detecter_Presence("Flop_3_Presence"))
                    {
                        Lire_Flop_3();
                    }

                    // =====================================
                    // TURN
                    // =====================================

                    if (Detecter_Presence("Turn_Presence"))
                    {
                        Lire_Turn();
                    }
                    if (Detecter_Presence("Turn_HB_Presence"))
                    {
                        Lire_Turn_HB();
                    }

                    // =====================================
                    // RIVER
                    // =====================================

                    if (Detecter_Presence("River_Presence"))
                    {
                        Lire_River();
                    }
                    if (Detecter_Presence("River_HB_Presence"))
                    {
                        Lire_River_HB();
                    }

                    // =====================================
                    // PAUSE CPU
                    // =====================================

                    await Task.Delay(1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                bTestBoard_Actif = false;
                bSuperviseurActif = false;
                Bt_Test_Board.Background =
                    Brushes.White;
            }
            bSuperviseurActif = false;
        }

        // =========================================================
        // TRIGGERS_BOARD
        // =========================================================
        private void Bt_Triggers_Board_Click(object sender, RoutedEventArgs e)
        {
            if (bTriggersBoard_Actif)
            {
                StopTousModes();
                return;
            }

            StopTousModes();

            bTriggersBoard_Actif = true;

            Rafraichir_Boutons_Modes();
        }

        // =========================================================
        // TAPIS PREFLOP
        // =========================================================
        private async void Bt_Tapis_PreFlop_Click(object sender, RoutedEventArgs e)
        {
            if (bTapisPreFlop_Actif)
            {
                StopTousModes();
                return;
            }

            StopTousModes();

            bTapisPreFlop_Actif = true;

            Rafraichir_Boutons_Modes();
        }

        // =========================================================
        // IA
        // =========================================================
        private async void Bt_IA_Click(object sender, RoutedEventArgs e)
        {
            // Arrêt du mode si déjà actif
            if (bIA_Actif)
            {
                StopTousModes();
                return;
            }

            // Arrêt de tous les autres modes
            StopTousModes();

            // Activation du mode IA
            bIA_Actif = true;

            // =====================================
            // RANGE PAR DÉFAUT = TAPIS
            // =====================================
            ActiverRange(Bt_Range_Tapis);

            // =====================================
            // INTERFACE
            // =====================================
            Rafraichir_Boutons_Modes();
        }

        private void Rafraichir_Boutons_Modes()
        {
            // =====================================
            // BOUTONS DES MODES
            // =====================================
            Bt_Test_Board.Background =
                bTestBoard_Actif
                    ? Brushes.Yellow
                    : Brushes.White;

            Bt_Triggers_Board.Background =
                bTriggersBoard_Actif
                    ? Brushes.Yellow
                    : Brushes.White;

            Bt_Tapis_PreFlop.Background =
                bTapisPreFlop_Actif
                    ? Brushes.Yellow
                    : Brushes.White;

            Bt_IA.Background =
                bIA_Actif
                    ? Brushes.Yellow
                    : Brushes.White;

            // =====================================
            // 🔥 RANGES IA
            // Visibles uniquement en mode IA
            // =====================================
            Visibility visibiliteRange =
                bIA_Actif
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            Bt_Range_Tapis.Visibility = visibiliteRange;
            Bt_Range_Suivre.Visibility = visibiliteRange;
            Bt_Range_Relance.Visibility = visibiliteRange;
        }
        #endregion

        #region SUPERVISEUR POKER

        private bool bSortieEnCours = false;
        bool bPosition_A_Recalculer = false;
        bool bAssise_En_Cours = false;
        // =========================================================
        // 🔥 SUPERVISEUR POKER
        // =========================================================
        private async Task Superviseur_Poker()
        {
            CaptureEcran();
            // =====================================================
            // SUPERVISEUR ACTIF
            // =====================================================
            bSuperviseurActif = true;

            // =====================================================
            // CHARGEMENT DES PREFERENCES
            // =====================================================
            Charger_Preferences_Utilisateur_JSON();

            // =====================================================
            // APPLICATION DES PREFERENCES
            // =====================================================
            Sl_Delai_Decision.Value = DELAI_PRISE_CONTROLE / 1000.0;

            if (bDebug)
                Mode_Debug_ON();
            else
                Mode_Debug_OFF();

            Bt_Se_Recave.Background =
                bSeRecaver
                    ? (Brush)FindResource("bleu")
                    : (Brush)FindResource("blanc");

            Appliquer_Niveau_Mise(PreferencesUtilisateur.NiveauMise);

            // =====================================================
            // 🔥 MEMO DE BASE
            // =====================================================
            Appliquer_Niveau_Memo(PreferencesUtilisateur.Memo);

            // =====================================================
            // 🔥 RESTAURATION RANGE TAPIS PERSONNALISÉ
            // Memo + ajouts manuels
            // =====================================================
            Charger_Range_Tapis_JSON();

            // =====================================================
            // 🔥 RAFRAÎCHIR L'AFFICHAGE DU RANGE
            // Après chargement du JSON
            // =====================================================
            RafraichirAffichage();

            // =====================================================
            // SAUVEGARDE DES PREFERENCES
            // =====================================================
            Sauver_Preferences_Utilisateur();

            Recherche_Place_Off();

            Reset_Lecture_Board();

            bGris = false;
            bAttente_Recave = false;
            bAutoriserHistorique = false;

            // =====================================================
            // NOUVELLE SESSION
            // =====================================================
            RTB_Historique.Document.Blocks.Clear();

            // =====================================================
            // MODES
            // Jaune = actif sans place
            // Vert  = actif avec place
            // =====================================================
            if (bTriggersBoard_Actif)
                Bt_Triggers_Board.Background = Brushes.Gold;
            else
                Bt_Triggers_Board.Background = Brushes.White;

            if (bTapisPreFlop_Actif)
                Bt_Tapis_PreFlop.Background = Brushes.Gold;
            else
                Bt_Tapis_PreFlop.Background = Brushes.White;

            if (bIA_Actif)
                Bt_IA.Background = Brushes.Gold;
            else
                Bt_IA.Background = Brushes.White;

            // =====================================================
            // 🔥 RANGES IA
            // Visibles uniquement en mode IA
            // =====================================================
            if (bIA_Actif)
            {
                Bt_Range_Tapis.Visibility = Visibility.Visible;
                Bt_Range_Suivre.Visibility = Visibility.Visible;
                Bt_Range_Relance.Visibility = Visibility.Visible;
            }
            else
            {
                Bt_Range_Tapis.Visibility = Visibility.Collapsed;
                Bt_Range_Suivre.Visibility = Visibility.Collapsed;
                Bt_Range_Relance.Visibility = Visibility.Collapsed;
            }
            // =====================================================
            // 🔥 DÉMARRAGE SUPERVISEUR
            // =====================================================

            // =====================================================
            // BOUCLE PRINCIPALE
            // =====================================================
            while (bSuperviseurActif)
            {
                CaptureEcran();
                await Task.Delay(1);
                // ==========================================
                // PRIORITE : FERMER LES PUBLICITES
                // ==========================================
                await Fermer_Publicite();

                // ==========================================
                // PRIORITE : FERMER AMITIE
                // ==========================================
                Fermer_Amitie();

                // ==========================================
                // 🔥 FERMER INFO JOUEUR
                // ==========================================
                Fermer_Info_Joueur();

                // ==========================================
                // 🔥 FERMER FELICITATIONS
                // ==========================================
                Fermer_Felicitation();

                // ==========================================
                // 🔥 RECAVE
                // AUTO OU MANUEL
                // ==========================================
                if (Plus_De_Jetons())
                {
                    Se_Recaver();

                    // ==========================================
                    // 🔥 NOUVELLE MAIN → RESET DES CARTES
                    // ==========================================
                    Reset_Lecture_Board();
                }

                // =================================
                // 🔴 VOYANT ROUGE
                // =================================
                if (!Table_Stabilisee())
                {
                    Validation_Moteur_Rouge();
                    bSuperviseurVert_Actif = false;
                }
                // =================================
                // 🟢 VOYANT VERT
                // =================================
                else
                {
                    Validation_Moteur_Vert();
                    bSuperviseurVert_Actif = true;
                }

                // =================================
                // 🔴 ROUGE → ON ATTEND
                // =================================
                if (!bSuperviseurVert_Actif)
                    continue;

                // ==========================================
                // 🔥 BARRE ESPACE
                // AUTO ↔ MANUEL
                // ==========================================
                GererBarreEspace();


                // =====================================
                // 🔥 MODE AUTOMATIQUE
                // → CHERCHER UNE PLACE SI DISPONIBLE
                // =====================================
                if (Un_Mode_Actif())
                {
                    CaptureEcran();

                    int place = Place_Libre();

                    if (place != 0)
                    {
                        await S_Assoir_A_Table();

                        // =====================================
                        // 🔥 NOUVELLE ASSISE
                        // → POSITION À RECALCULER AU VERT
                        // =====================================

                        if (!Pupitre_Gris())
                        {
                            bPosition_A_Recalculer = true;
                        }
                    }
                }

                // =====================================
                // 🔴 DÉCISION PAR DÉFAUT
                // =====================================
                ActionForcee = "PAROLE";
                // ==========================
                // MAIN 1
                // ==========================
                if (!bMain1_Lu && Detecter_Presence("Main_1_Presence"))
                {
                    Debug("Lecture Main1");
                    // =====================================
                    // NOUVELLE MAIN
                    // =====================================
                    Bt_J_ai_Gagne.Background = Brushes.White;

                    // =====================================
                    // 🔥 RESET ANNONCE PASSE
                    // Une seule annonce par main
                    // =====================================
                    bPasse_Annonce = false;

                    // =====================================
                    // RESET BOUTON TAPIS
                    // =====================================
                    Bt_Tapis.Background = Blanc;

                    // =====================================
                    // RESET BOUTON PAROLE / PASSE
                    // =====================================
                    Bt_Parole_Passe.Background = Blanc;

                    // =====================================
                    // 🔥 RESET DANGER
                    // Nouveau coup
                    // =====================================
                    StopFlashDanger();

                    Bt_Danger.Background = Blanc;
                    Bt_Danger.Foreground = Brushes.Black;
                    Bt_Danger.Content = "";

                    // =====================================
                    // 🔥 RESET ACTION FORCEE
                    // Nouvelle main
                    // =====================================
                    ActionForcee = "PAROLE";

                    // =====================================
                    // 🔥 RANDOM MEMO
                    // Tirage une seule fois par Main
                    // =====================================
                    AppliquerRandomSiActif();

                    bAttendrePosition = false;
                    Effacer_Gagnants();

                    // =====================================
                    // POSITION + NIVEAU DE TABLE
                    // UNE SEULE FOIS PAR MAIN
                    // =====================================
                    Lire_Ma_Position();
                    Lire_Niveau_Table();

                    Lire_Main_1();

                    bMain1_Lu = true;

                    continue;
                }

                // ==========================
                // MAIN 2
                // ==========================
                if (bMain1_Lu && !bMain2_Lu && Detecter_Presence("Main_2_Presence"))
                {
                    Lire_Main_2();

                    // =====================================
                    // 🔥 TEST RANGE TAPIS
                    // Toujours le vrai Range Tapis
                    // indépendamment du Range affiché
                    // =====================================
                    string main = NormaliserMainTapis(T_Carte[0], T_Carte[1]);

                    bool dansRangeTapis = !string.IsNullOrEmpty(main) && RangeTapis.Contains(main);

                    // =====================================
                    // BOUTON TAPIS
                    // =====================================
                    if (dansRangeTapis)
                    {
                        Bt_Tapis.Background = Vert;
                    }
                    else
                    {
                        Bt_Tapis.Background = Blanc;
                    }

                    // =====================================
                    // 🔥 ACTION TAPIS
                    // Avec ou sans mode Tapis PréFlop
                    // =====================================
                    if (dansRangeTapis)
                    {
                        ActionForcee = "TAPIS";
                    }

                    bMain2_Lu = true;


                    // =====================================
                    // MODES : JAUNE → VERT
                    // =====================================
                    if (MaPlace != 0)
                    {
                        if (bTriggersBoard_Actif)
                            Bt_Triggers_Board.Background = Brushes.LimeGreen;

                        if (bTapisPreFlop_Actif)
                            Bt_Tapis_PreFlop.Background = Brushes.LimeGreen;

                        if (bIA_Actif)
                            Bt_IA.Background = Brushes.LimeGreen;
                    }

                    // =====================================
                    // CALCUL PRE-FLOP
                    // =====================================
                    bPreFlop_Calcul = true;

                    //await Task.Delay(5);
                    Calcul_PreFlop();

                    // =====================================
                    // 🔥 SON COMBINAISON PRE-FLOP
                    // =====================================
                    if (bPreFlop_Suite && bPreFlop_Pareille)
                        JouerSon("Suite Pareille.wav");
                    else if (bPreFlop_Suite)
                        JouerSon("Suite.wav");
                    else if (bPreFlop_Pareille)
                        JouerSon("Pareille.wav");
                    else if (bPreFlop_Paire)
                        JouerSon("Paire.wav");

                    bPreFlop_Calcul = false;

                    bPreFlop_Paire = false;
                    bPreFlop_Pareille = false;
                    bPreFlop_Suite = false;
                    // =====================================
                    // 🔴 APRÈS MAIN2
                    // La suite sera traitée ici
                    // =====================================
                    Capture_ShareX();
                }

                // =================================
                // 🔴 LE VOYANT PASSE AU ROUGE
                // =================================

                if (!Table_Stabilisee())
                {
                    // =================================
                    // PREMIÈRE DÉTECTION DU ROUGE
                    // =================================
                    if (!bVoyantRougeAttente)
                    {
                        bVoyantRougeAttente = true;
                        VoyantRouge = DateTime.Now;
                    }
                    else
                    {
                        // =================================
                        // 🔥 ROUGE DEPUIS 2.5 SECONDES
                        // =================================
                        if ((DateTime.Now - VoyantRouge).TotalSeconds >= 2.5)
                        {
                            // =================================
                            // 🔴 PASSER LE VOYANT VISUEL AU ROUGE
                            // =================================
                            Validation_Moteur_Rouge();

                            // =================================
                            // 🔒 BLOQUER LE SUPERVISEUR
                            // =================================
                            bSuperviseurVert_Actif = false;
                        }
                    }
                }
                else
                {
                    // =================================
                    // 🟢 RETOUR AU VERT
                    // =================================
                    bVoyantRougeAttente = false;
                    VoyantRouge = DateTime.MinValue;
                }


                // =====================================================
                // 🟢 À PARTIR D'ICI : SUPERVISEUR AUTORISÉ
                // =====================================================

                // =====================================================
                // 🔥 POSITION ACTUELLE
                // AUTO OU MANUEL
                // =====================================================
                int positionActuelle = MaPlace;

                if (!Pupitre_Gris())
                {
                    positionActuelle = Suis_Je_Assis();
                }

                // =====================================================
                // 🔴 PAS ASSIS
                // =====================================================
                if (positionActuelle == 0)
                {
                    // =====================================
                    // JE N'AI PLUS DE PLACE
                    // =====================================
                    MaPlace = 0;

                    // =====================================
                    // NOUVELLE ASSISE
                    // → NOUVELLE STABILISATION
                    // =====================================
                    bSuperviseurVert_Actif = false;

                    ResetPosition();
                    Reset_Lecture_Board();
                    Reset_Calculs();
                    StopFlashDanger();

                    Bt_Danger.Background =
                        Blanc;

                    Bt_Danger.Foreground =
                        Brushes.Black;

                    Bt_Danger.Content =
                        "";

                    if (bTriggersBoard_Actif)
                        Bt_Triggers_Board.Background =
                            Brushes.Gold;

                    if (bTapisPreFlop_Actif)
                        Bt_Tapis_PreFlop.Background =
                            Brushes.Gold;

                    if (bIA_Actif)
                        Bt_IA.Background =
                            Brushes.Gold;

                    continue;
                }

                // =====================================================
                // 🟢 ASSIS
                // =====================================================
                MaPlace = positionActuelle;

                // =====================================================
                // 🔥 VALIDATION TABLE
                // =====================================================
                if (!bSuperviseurVert_Actif)
                {
                    if (Table_Stabilisee())
                    {
                        bSuperviseurVert_Actif = true;

                        Validation_Moteur_Vert();
                    }
                }

                // =====================================================
                // 🔥 J'AI GAGNÉ → QUITTER ET REVENIR
                // =====================================================
                if (bQuitterRevenir && !bSortieEnCours)
                {
                    bSortieEnCours = true;

                    // =====================================
                    // MODES : VERT → JAUNE
                    // =====================================
                    if (bTriggersBoard_Actif)
                        Bt_Triggers_Board.Background = Brushes.Gold;

                    if (bTapisPreFlop_Actif)
                        Bt_Tapis_PreFlop.Background = Brushes.Gold;

                    if (bIA_Actif)
                        Bt_IA.Background = Brushes.Gold;

                    // =====================================
                    // QUITTER MA PLACE
                    // =====================================
                    Clic(890, 190);
                    await Task.Delay(100);

                    // =====================================
                    // ACCELERER LE DEPART
                    // =====================================
                    Clic(990, 250);
                    await Task.Delay(50);

                    Clic(990, 250);

                    // =====================================
                    // ATTENDRE LA SORTIE REELLE
                    // =====================================
                    while (!Je_Ne_Suis_Pas_Assis())
                    {
                        await Task.Delay(10);
                        CaptureEcran();
                    }

                    // =====================================
                    // JE N'AI PLUS DE PLACE
                    // =====================================
                    MaPlace = 0;

                    bSuperviseurVert_Actif = false;

                    ResetPosition();
                    Reset_Table();

                    // =====================================
                    // RECAVE APRES RETOUR
                    // =====================================
                    if (Plus_De_Jetons())
                    {
                        Se_Recaver();

                        await Task.Delay(300);

                        CaptureEcran();
                    }

                    // =====================================
                    // NOUVELLE STABILISATION
                    // =====================================
                    bSuperviseurVert_Actif = false;

                    while (!Table_Stabilisee())
                    {
                        await Task.Delay(1);
                        CaptureEcran();
                    }

                    // =====================================
                    // MOTEUR VALIDE
                    // =====================================
                    Validation_Moteur_Vert();

                    bSuperviseurVert_Actif = true;

                    // =====================================
                    // FIN DU VERROU DE GAIN
                    // =====================================
                    bQuitterRevenir = false;
                    bSortieEnCours = false;

                    continue;
                }

                // =====================================================
                // 🔥 DÉCISION PRÉFLOP
                // AVANT LA BARRIÈRE BOARD
                // =====================================================

                if (!bFlop1_Lu &&
                    Mode_Decision())
                {
                    // =====================================
                    // 1 - PASSAGE AVANT BARRIÈRE
                    // =====================================
                    T_Stak_Depart_Joueur1.Text = "AVANT ✓";

                    // =====================================
                    // 2 - MAIN1
                    // =====================================
                    T_Stak_Depart_Joueur2.Text = "M1 ✓";

                    // =====================================
                    // 3 - MAIN2
                    // =====================================
                    T_Stak_Depart_Joueur3.Text = "M2 ✓";

                    // =====================================
                    // 4 - MODE DECISION
                    // =====================================
                    T_Stak_Depart_Joueur4.Text = "MODE ✓";

                    // =====================================
                    // ATTENTE DE DECISION
                    // =====================================
                    if (!bAttente_Decision)
                    {
                        bAttente_Decision = true;
                        Heure_Decision = DateTime.Now;
                    }

                    // =====================================
                    // 5 - DECISION AUTORISEE
                    // =====================================
                    if (Decision_Autorisee())
                    {
                        T_Stak_Depart_Joueur5.Text = "AUT ✓";

                        // =====================================
                        // MAIN ACTUELLE
                        // =====================================
                        string main =
                            NormaliserMainTapis(
                                T_Carte[0],
                                T_Carte[1]);

                        // =====================================
                        // RANGE TAPIS
                        // =====================================
                        bool dansRangeTapis =
                            !string.IsNullOrEmpty(main) &&
                            RangeTapis.Contains(main);

                        // =====================================
                        // PRIORITÉ TAPIS
                        // =====================================
                        if (dansRangeTapis)
                        {
                            ActionForcee = "TAPIS";
                        }
                        else
                        {
                            ActionForcee = Action_Mode_Actif();
                        }

                        // =====================================
                        // 6 - ACTION FORCEE
                        // =====================================
                        T_Stak_Depart_Joueur6.Text =
                            "ACT " + ActionForcee;

                        // =====================================
                        // 7 - DECISION()
                        // =====================================
                        T_Stak_Depart_Joueur7.Text = "DEC ✓";

                        if (Decision())
                        {
                            // =================================
                            // 8 - CLIC EFFECTUE
                            // =================================
                            T_Stak_Depart_Joueur8.Text = "CLIC ✓";

                            // =================================
                            // COMPTEUR TAPIS
                            // =================================
                            if (ActionForcee == "TAPIS")
                            {
                                NbTapis++;

                                LbNbTapis.Content =
                                    NbTapis.ToString();
                            }

                            // =================================
                            // RESET ACTION FORCEE
                            // =================================
                            ActionForcee = "";

                            // =================================
                            // LAISSER POKERTEAM TERMINER
                            // =================================
                            await Task.Delay(300);

                            CaptureEcran();

                            // =================================
                            // 9 - ATTENTE DISPARITION
                            // =================================
                            T_Stak_Depart_Joueur9.Text = "ATT ✓";

                            continue;
                        }
                    }
                }
                else if (!Mode_Decision())
                {
                    bAttente_Decision = false;
                }

                // =========================================================
                // 🔴 BARRIÈRE BOARD
                // TOUT CE QUI SUIT EST INTERDIT TANT QUE ROUGE
                // =========================================================
                if (!bSuperviseurVert_Actif)
                {
                    continue;
                }


                // ==========================================
                // AUCUNE TABLE
                // ==========================================
                if (Aucune_Table())
                {
                    if (!bQuitterRevenir)
                    {
                        bSuperviseurVert_Actif = false;

                        Validation_Moteur_Rouge();

                        Reset_Table();
                        ResetPosition();
                        ResetNiveauTable();
                    }

                    await Task.Delay(50);

                    continue;
                }


                // ==========================================
                // PAUSE
                // ==========================================
                if (JeuEnPause)
                {
                    await Task.Delay(100);

                    continue;
                }

                // =====================================================
                // 🟢 À PARTIR D'ICI
                // MOTEUR BOARD AUTORISÉ
                // =====================================================


                // ==========================================
                // PUPITRE GRIS
                // ==========================================
                if (Pupitre_Gris())
                {
                    bGris = true;

                    // ==========================================
                    // PLUS DE POT
                    // ==========================================
                    if (Plus_De_Pot())
                    {
                        if (!bAttente_Recave)
                        {
                            bAttente_Recave = true;
                            Heure_Recave = DateTime.Now;
                        }

                        // =====================================
                        // 🔥 J'AI GAGNÉ ?
                        // Seulement si Se Recaver = ON
                        // =====================================
                        if (bSeRecaver)
                        {
                            bool gagne = J_Ai_Gagne();

                            if (gagne)
                            {
                                Bt_J_ai_Gagne.Background = (Brush)FindResource("vert1");

                                // =====================================
                                // 🔊 SON GAGNE
                                // =====================================
                                JouerSon("Gagne.wav");

                                bQuitterRevenir = true;
                            }
                        }
                    }
                }

                // ==========================
                // RETOUR DEBUT
                // ==========================
                if (bGris && !Pupitre_Gris())
                {
                    bGris = false;

                    // ==========================
                    // HISTORIQUE
                    // ==========================
                    if (bAutoriserHistorique && bMain1_Lu && bMain2_Lu)
                    {
                        Ajouter_Historique();

                        NbMains += 1;
                        LbNbParole.Content = NbMains.ToString();
                    }

                    // ==========================
                    // REMISE A ZERO
                    // ==========================
                    Reset_Lecture_Board();

                    Reset_Table();

                    continue;
                }

                // ==========================
                // FLOP 1
                // ==========================
                if (bMain2_Lu && !bFlop1_Lu && Detecter_Presence("Flop_1_Presence"))
                {
                    await Task.Delay(2);
                    Lire_Flop_1();

                    bFlop1_Lu = true;

                    continue;
                }

                // ==========================
                // FLOP 2
                // ==========================
                if (bFlop1_Lu && !bFlop2_Lu && Detecter_Presence("Flop_2_Presence"))
                {
                    Lire_Flop_2();

                    bFlop2_Lu = true;

                    continue;
                }
                // ==========================
                // FLOP 3
                // ==========================
                if (bFlop2_Lu && !bFlop3_Lu && Detecter_Presence("Flop_3_Presence"))
                {

                    Lire_Flop_3();

                    await Task.Delay(2);
                    // =====================================
                    // 🔥 TAPIS OFF
                    // Dès que le Flop 3 est lu
                    // =====================================
                    Bt_Tapis.Background = Blanc;

                    bFlop3_Lu = true;

                    // =====================================
                    // CALCUL COMBINAISON FLOP
                    // =====================================
                    bFlop_Calcul = true;

                    Calcul_Flop();

                    // =====================================
                    // 🔥 SON COMBINAISON FLOP
                    // =====================================
                    if (bFlop_QFlush)
                        JouerSon("Quinte Flush.wav");
                    else if (bFlop_Carre)
                        JouerSon("Carre.wav");
                    else if (bFlop_Full)
                        JouerSon("Full.wav");
                    else if (bFlop_Couleur)
                        JouerSon("Couleur.wav");
                    else if (bFlop_Quinte)
                        JouerSon("Quinte.wav");
                    else if (bFlop_Brelan)
                        JouerSon("Brelan.wav");
                    else if (bFlop_2_Paires)
                        JouerSon("Deux Paires.wav");
                    else if (bFlop_Paire)
                        JouerSon("Paire.wav");

                    bFlop_Calcul = false;

                    // =====================================
                    // RESET ANCIENS CALCULS COMBINAISON
                    // =====================================
                    bFlop_Carre = false;
                    bFlop_Full = false;
                    bFlop_Quinte = false;
                    bFlop_Couleur = false;
                    bFlop_Brelan = false;
                    bFlop_2_Paires = false;
                    bFlop_Paire = false;

                    // =====================================
                    // CALCUL TIRAGES + DANGERS FLOP
                    // =====================================
                    Calcul_Tirages();

                    // =====================================
                    // SON TIRAGE FLOP
                    // =====================================
                    if (TirageFlop.QuinteFlush)
                    {
                        JouerSon("Tirage Quinte Flush.wav");
                    }
                    else if (TirageFlop.Couleur)
                    {
                        JouerSon("Tirage Couleur.wav");
                    }
                    else if (TirageFlop.Quinte)
                    {
                        if (TirageFlop.Ventrale)
                            JouerSon("Ventrale.wav");
                        else
                            JouerSon("Tirage Quinte.wav");
                    }

                    // =====================================
                    // DANGER FLOP
                    // =====================================
                    Afficher_Danger(TirageFlop);

                    // =====================================
                    // SON DANGER FLOP
                    // =====================================
                    if (TirageFlop.DangerCarre)
                    {
                        JouerSon("Danger Carre.wav");
                    }
                    else if (TirageFlop.DangerFull)
                    {
                        JouerSon("Danger Full.wav");
                    }
                    else if (TirageFlop.DangerQuinteFlush)
                    {
                        JouerSon("Danger Quinte Flush.wav");
                    }
                    else if (TirageFlop.DangerCouleur &&
                             TirageFlop.DangerQuinte)
                    {
                        JouerSon("Danger Couleur Quinte.wav");
                    }
                    else if (TirageFlop.DangerCouleur)
                    {
                        JouerSon("Danger Couleur.wav");
                    }
                    else if (TirageFlop.DangerQuinte)
                    {
                        JouerSon("Danger Quinte.wav");
                    }

                    // =====================================
                    // FIN TRAITEMENT FLOP
                    // =====================================

                    // ==========================
                    // Passage au vert
                    // ==========================
                    if (!bAutoriserHistorique &&
                        !string.IsNullOrEmpty(TB_F1.Text) &&
                        !string.IsNullOrEmpty(TB_F2.Text) &&
                        !string.IsNullOrEmpty(TB_F3.Text))
                    {
                        bAutoriserHistorique = true;

                        if (bTriggersBoard_Actif)
                            Bt_Triggers_Board.Background =
                                Brushes.LightGreen;

                        if (bTapisPreFlop_Actif)
                            Bt_Tapis_PreFlop.Background =
                                Brushes.LightGreen;

                        if (bIA_Actif)
                            Bt_IA.Background =
                                Brushes.LightGreen;
                    }

                    // =====================================
                    // 🔥 FIN DE MAIN AU FLOP
                    // Pupitre gris
                    //
                    // On ne traite pas ici l'historique :
                    // le bloc général Pupitre_Gris()
                    // s'en chargera au tour suivant.
                    // =====================================
                    if (Pupitre_Gris())
                    {
                        bGris = true;
                    }
                    // =====================================
                    // 🔥 RESET AFFICHAGE DANGER FLOP
                    // Nouvelle lecture du Flop
                    // =====================================
                    StopFlashDanger();

                    Bt_Danger.Background = Blanc;
                    Bt_Danger.Foreground = Brushes.Black;
                    Bt_Danger.Content = "";

                    continue;
                }

                // ==========================
                // TURN
                // ==========================
                if (bFlop3_Lu && !bTurn_Lu)
                {
                    // ----- Turn normal -----
                    if (Detecter_Presence("Turn_Presence"))
                    {

                        await Task.Delay(1);
                        Lire_Turn();

                        await Task.Delay(10);
                        bTurn_Lu = true;
                    }

                    // ----- Turn HB -----
                    if (Detecter_Presence("Turn_HB_Presence"))
                    {

                        await Task.Delay(5);
                        Lire_Turn_HB();

                        await Task.Delay(5);

                        bTurn_Lu = true;
                    }

                    // ----- Traitement commun -----
                    if (bTurn_Lu)
                    {
                        // =====================================
                        // 🔥 NOUVEAU TURN
                        // Effacer le Danger du Flop
                        // =====================================
                        StopFlashDanger();

                        Bt_Danger.Background = Blanc;
                        Bt_Danger.Foreground = Brushes.Black;
                        Bt_Danger.Content = "";

                        bTurn_Lu = true;
                        // =====================================
                        // CALCUL COMBINAISON TURN
                        // =====================================
                        bTurn_Calcul = true;

                        Calcul_Turn();

                        // =====================================
                        // 🔥 SON COMBINAISON TURN
                        // =====================================
                        if (bTurn_QFlush)
                            JouerSon("Quinte Flush.wav");
                        else if (bTurn_Carre)
                            JouerSon("Carre.wav");
                        else if (bTurn_Full)
                            JouerSon("Full.wav");
                        else if (bTurn_Couleur)
                            JouerSon("Couleur.wav");
                        else if (bTurn_Quinte)
                            JouerSon("Quinte.wav");
                        else if (bTurn_Brelan)
                            JouerSon("Brelan.wav");
                        else if (bTurn_2_Paires)
                            JouerSon("Deux Paires.wav");
                        else if (bTurn_Paire)
                            JouerSon("Paire.wav");

                        bTurn_Calcul = false;

                        // =====================================
                        // RESET ANCIENS CALCULS COMBINAISON
                        // =====================================
                        bTurn_Carre = false;
                        bTurn_Full = false;
                        bTurn_Quinte = false;
                        bTurn_Couleur = false;
                        bTurn_Brelan = false;
                        bTurn_2_Paires = false;
                        bTurn_Paire = false;

                        // =====================================
                        // CALCUL TIRAGES + DANGERS TURN
                        // =====================================
                        await Task.Delay(3);

                        Calcul_Tirages();

                        // =====================================
                        // 🔥 SON TIRAGE TURN
                        // =====================================
                        if (TirageTurn.QuinteFlush)
                        {
                            JouerSon("Tirage Quinte Flush.wav");
                        }
                        else if (TirageTurn.Couleur)
                        {
                            JouerSon("Tirage Couleur.wav");
                        }
                        else if (TirageTurn.Quinte)
                        {
                            if (TirageTurn.Ventrale)
                                JouerSon("Ventrale.wav");
                            else
                                JouerSon("Tirage Quinte.wav");
                        }

                        // =====================================
                        // 🔥 DANGER TURN
                        // =====================================
                        Afficher_Danger(TirageTurn);

                        // =====================================
                        // 🔥 SON DANGER TURN
                        // =====================================
                        if (TirageTurn.DangerCarre)
                        {
                            JouerSon("Danger Carre.wav");
                        }
                        else if (TirageTurn.DangerFull)
                        {
                            JouerSon("Danger Full.wav");
                        }
                        else if (TirageTurn.DangerQuinteFlush)
                        {
                            JouerSon("Danger Quinte Flush.wav");
                        }
                        else if (TirageTurn.DangerCouleur &&
                                 TirageTurn.DangerQuinte)
                        {
                            JouerSon("Danger Couleur Quinte.wav");
                        }
                        else if (TirageTurn.DangerCouleur)
                        {
                            JouerSon("Danger Couleur.wav");
                        }
                        else if (TirageTurn.DangerQuinte)
                        {
                            JouerSon("Danger Quinte.wav");
                        }

                        continue;
                    }
                }
                // ==========================
                // RIVER
                // ==========================
                if (bTurn_Lu && !bRiver_Lu)
                {
                    // ----- River normale -----
                    if (Detecter_Presence("River_Presence"))
                    {
                        Lire_River();

                        // =====================================
                        // 🔥 NOUVELLE RIVER
                        // Effacer le Danger du TURN
                        // =====================================
                        StopFlashDanger();

                        Bt_Danger.Background = Blanc;
                        Bt_Danger.Foreground = Brushes.Black;
                        Bt_Danger.Content = "";

                        bRiver_Lu = true;
                        //Capture_ShareX();
                    }

                    // ----- River HB -----
                    if (Detecter_Presence("River_HB_Presence"))
                    {

                        Lire_River_HB();

                        // =====================================
                        // 🔥 NOUVELLE RIVER
                        // Effacer le Danger du TURN
                        // =====================================
                        StopFlashDanger();

                        Bt_Danger.Background = Blanc;
                        Bt_Danger.Foreground = Brushes.Black;
                        Bt_Danger.Content = "";

                        bRiver_Lu = true;
                    }

                    // ----- Traitement commun -----
                    if (bRiver_Lu)
                    {

                        Clic(1108, 844);

                        bRiver_Calcul = true;

                        // =====================================
                        // CALCUL COMBINAISON RIVER
                        // =====================================
                        Calcul_River();

                        // =====================================
                        // 🔥 SON COMBINAISON RIVER
                        // =====================================
                        if (bRiver_QFlush)
                            JouerSon("Quinte Flush.wav");
                        else if (bRiver_Carre)
                            JouerSon("Carre.wav");
                        else if (bRiver_Full)
                            JouerSon("Full.wav");
                        else if (bRiver_Couleur)
                            JouerSon("Couleur.wav");
                        else if (bRiver_Quinte)
                            JouerSon("Quinte.wav");
                        else if (bRiver_Brelan)
                            JouerSon("Brelan.wav");
                        else if (bRiver_2_Paires)
                            JouerSon("Deux Paires.wav");
                        else if (bRiver_Paire)
                            JouerSon("Paire.wav");


                        bRiver_Calcul = false;

                        // =====================================
                        // RESET CALCUL RIVER
                        // =====================================
                        bRiver_Carre = false;
                        bRiver_Full = false;
                        bRiver_Quinte = false;
                        bRiver_Couleur = false;
                        bRiver_Brelan = false;
                        bRiver_2_Paires = false;
                        bRiver_Paire = false;

                        // =====================================
                        // CALCUL TIRAGES + DANGERS RIVER
                        // =====================================
                        Calcul_Tirages();

                        // =====================================
                        // DANGER RIVER
                        // =====================================
                        Afficher_Danger(TirageRiver);

                        // =====================================
                        // 🔥 SON DANGER RIVER
                        // =====================================
                        if (TirageRiver.DangerCarre)
                        {
                            JouerSon("Danger Carre.wav");
                        }
                        else if (TirageRiver.DangerFull)
                        {
                            JouerSon("Danger Full.wav");
                        }
                        else if (TirageRiver.DangerQuinteFlush)
                        {
                            JouerSon("Danger Quinte Flush.wav");
                        }
                        else if (TirageRiver.DangerCouleur &&
                                 TirageRiver.DangerQuinte)
                        {
                            JouerSon("Danger Couleur Quinte.wav");
                        }
                        else if (TirageRiver.DangerCouleur)
                        {
                            JouerSon("Danger Couleur.wav");
                        }
                        else if (TirageRiver.DangerQuinte)
                        {
                            JouerSon("Danger Quinte.wav");
                        }

                        Capture_ShareX();

                        continue;
                    }
                }

                // ==========================
                // RETOUR DEBUT
                // ==========================
                if (bGris && !Pupitre_Gris())
                {
                    bGris = false;

                    // ==========================
                    // HISTORIQUE
                    // ==========================
                    if (bAutoriserHistorique && bMain1_Lu && bMain2_Lu)
                    {
                        Ajouter_Historique();

                        NbMains += 1;
                        LbNbParole.Content = NbMains.ToString();
                    }

                    // ==========================
                    // REMISE A ZERO
                    // ==========================
                    Reset_Lecture_Board();
                    Reset_Table();

                    continue;
                }

            }



        }

        // =========================================================
        // 🔥 ESPACE AUTO / MANUEL
        // =========================================================
        private bool bEspaceEnfonce = false;

        // =====================================
        // CAPTURE SHAREX
        // =====================================
        private void Capture_ShareX()
        {
            Clic(570, 824);   // sélection éventuelle de l'écran actif
            Clic(501, 778);   // lancement de la capture
        }

        // =========================================================
        // 🔥 TEST BARRE ESPACE
        // Une seule bascule par pression
        // =========================================================
        private void GererBarreEspace()
        {
            bool espaceEnfonce =
                (GetAsyncKeyState(VK_SPACE) & 0x8000) != 0;

            if (espaceEnfonce && !bEspaceEnfonce)
            {
                bEspaceEnfonce = true;

                BasculerAutoManuel();
            }

            if (!espaceEnfonce)
            {
                bEspaceEnfonce = false;
            }
        }
        private bool Un_Mode_Actif()
        {
            return bTriggersBoard_Actif
                || bTapisPreFlop_Actif
                || bIA_Actif;
        }

        private void Appliquer_Preferences_Utilisateur()
        {
            bDebug = PreferencesUtilisateur.ModeDebug;
            DELAI_PRISE_CONTROLE = PreferencesUtilisateur.DelaiPriseControle;

            Sl_Delai_Decision.Value = DELAI_PRISE_CONTROLE / 1000.0;

            if (bDebug)
                Mode_Debug_ON();
            else
                Mode_Debug_OFF();
        }

        private void Bt_Danger_Click(object sender, RoutedEventArgs e)
        {

        }

        // =========================================================
        // 🔥 QUITTER ET REVENIR
        // Demande universelle : avec ou sans mode
        // =========================================================
        private void Bt_Quitter_Et_Revenir_Click(object sender, RoutedEventArgs e)
        {
            bQuitterRevenir = true;
        }

        private void Appliquer_Niveau_Mise(int niveau)
        {
            switch (niveau)
            {
                case -9: ActiverMise(Bt_M9); break;
                case -8: ActiverMise(Bt_M8); break;
                case -6: ActiverMise(Bt_M6); break;
                case -4: ActiverMise(Bt_M4); break;
                case -2: ActiverMise(Bt_M2); break;

                case 0: ActiverMise(Bt_0); break;

                case 2: ActiverMise(Bt_P2); break;
                case 4: ActiverMise(Bt_P4); break;
                case 6: ActiverMise(Bt_P6); break;
                case 8: ActiverMise(Bt_P8); break;
                case 10: ActiverMise(Bt_P10); break;
            }
        }

        // =========================================================
        // 🔥 RANGE TAPIS
        // =========================================================
        private void Bt_Range_Tapis_Click(object sender, RoutedEventArgs e)
        {
            ActiverRange(Bt_Range_Tapis);
        }

        // =========================================================
        // 🔥 RANGE RELANCE
        // =========================================================
        private void Bt_Range_Relance_Click(object sender, RoutedEventArgs e)
        {
            ActiverRange(Bt_Range_Relance);
        }

        // =========================================================
        // 🔥 RANGE SUIVRE
        // =========================================================
        private void Bt_Range_Suivre_Click(object sender, RoutedEventArgs e)
        {
            ActiverRange(Bt_Range_Suivre);
        }

        // =========================================================
        // 🔥 APPLIQUER MEMO
        // =========================================================
        private void Appliquer_Niveau_Memo(int memo)
        {
            switch (memo)
            {
                case 1: Bt_Mem01_Click(null, null); break;
                case 2: Bt_Mem02_Click(null, null); break;
                case 3: Bt_Mem03_Click(null, null); break;
                case 4: Bt_Mem04_Click(null, null); break;
                case 5: Bt_Mem05_Click(null, null); break;
                case 6: Bt_Mem06_Click(null, null); break;
                case 7: Bt_Mem07_Click(null, null); break;
                case 8: Bt_Mem08_Click(null, null); break;
                case 9: Bt_Mem09_Click(null, null); break;
                case 10: Bt_Mem10_Click(null, null); break;
                case 11: Bt_Mem11_Click(null, null); break;
                case 12: Bt_Mem12_Click(null, null); break;
                case 13: Bt_Mem13_Click(null, null); break;
            }
        }

        private void Bt_Test_Click(object sender, RoutedEventArgs e)
        {
            bModeTest = !bModeTest;

            if (bModeTest)
            {
                // ==========================
                // MODE TEST ON
                // ==========================
                bSuperviseurActif = false;

                bTriggersBoard_Actif = false;
                bTapisPreFlop_Actif = false;
                bIA_Actif = false;

                Bt_Test.Background = Orange;
            }
            else
            {
                // ==========================
                // MODE TEST OFF
                // ==========================
                bModeTest = false;

                Bt_Test.Background = Blanc;

                bSuperviseurActif = true;

                _ = Superviseur_Poker();
            }
        }

        private int Nombre_Places_Libres()
        {
            int nb = 0;

            for (int i = 0; i < 18; i += 2)
            {
                int c1 = LirePixel(
                    PlacesLibresDetect[i].X,
                    PlacesLibresDetect[i].Y);

                int c2 = LirePixel(
                    PlacesLibresDetect[i + 1].X,
                    PlacesLibresDetect[i + 1].Y);

                // =====================================
                // 🟠 UN OU L'AUTRE DES DEUX PIXELS
                // =====================================
                if (c1 == PlacesLibresDetect[i].Couleur ||
                    c2 == PlacesLibresDetect[i + 1].Couleur)
                {
                    nb++;
                }
            }

            return nb;
        }

        private bool Validation_Flop()
        {
            // =====================================
            // Remise à zéro
            // =====================================
            Flop_1 = "";
            Flop_2 = "";
            Flop_3 = "";

            // =====================================
            // FLOP 1
            // =====================================
            if (Detecter_Presence("Flop_1_Presence"))
            {
                Lire_Flop_1_Validation();
            }

            // =====================================
            // FLOP 2
            // =====================================
            if (Detecter_Presence("Flop_2_Presence"))
            {
                Lire_Flop_2_Validation();
            }

            // =====================================
            // FLOP 3
            // =====================================
            if (Detecter_Presence("Flop_3_Presence"))
            {
                Lire_Flop_3_Validation();
            }

            // =====================================
            // VALIDATION
            // =====================================
            if (Flop_1 != "" &&
                Flop_2 != "" &&
                Flop_3 != "")
            {
                return true;
            }

            return false;
        }
        // =========================================================
        // LIRE FLOP 1 (VALIDATION)
        // =========================================================
        private void Lire_Flop_1_Validation()
        {
            Flop_1 = "";

            foreach (var carte in Flop1Detect)
            {
                if (string.IsNullOrEmpty(carte.Nom))
                    continue;

                System.Drawing.Color pixel =
                    Bitmap_Main.GetPixel(
                        carte.X,
                        carte.Y);

                int couleurLue =
                    (pixel.R << 16) |
                    (pixel.G << 8) |
                     pixel.B;

                if (couleurLue == carte.Couleur)
                {
                    Flop_1 = carte.Nom;
                    return;
                }
            }
        }
        // =========================================================
        // LIRE FLOP 2 (VALIDATION)
        // =========================================================
        private void Lire_Flop_2_Validation()
        {
            Flop_2 = "";

            foreach (var carte in Flop2Detect)
            {
                if (string.IsNullOrEmpty(carte.Nom))
                    continue;

                System.Drawing.Color pixel =
                    Bitmap_Main.GetPixel(
                        carte.X,
                        carte.Y);

                int couleurLue =
                    (pixel.R << 16) |
                    (pixel.G << 8) |
                     pixel.B;

                if (couleurLue == carte.Couleur)
                {
                    Flop_2 = carte.Nom;
                    return;
                }
            }
        }
        // =========================================================
        // LIRE FLOP 3 (VALIDATION)
        // =========================================================
        private void Lire_Flop_3_Validation()
        {
            Flop_3 = "";

            foreach (var carte in Flop3Detect)
            {
                if (string.IsNullOrEmpty(carte.Nom))
                    continue;

                System.Drawing.Color pixel =
                    Bitmap_Main.GetPixel(
                        carte.X,
                        carte.Y);

                int couleurLue =
                    (pixel.R << 16) |
                    (pixel.G << 8) |
                     pixel.B;

                if (couleurLue == carte.Couleur)
                {
                    Flop_3 = carte.Nom;
                    return;
                }
            }
        }
        private bool Validation_Moteur_Rouge()
        {
            Voyant_Validation_Moteur.Background = Brushes.Red;
            Voyant_Validation_Moteur.Foreground = Brushes.White;
            return true;
        }

        private bool Validation_Moteur_Vert()
        {
            Voyant_Validation_Moteur.Background = Brushes.LightGreen;
            Voyant_Validation_Moteur.Foreground = Brushes.Black;
            return true;
        }
        private void Recherche_Place_Off()
        {
            Voyant_Recherche_Place.Visibility = Visibility.Collapsed;
        }

        private void Recherche_Place_On()
        {
            Voyant_Recherche_Place.Background = Brushes.Orange;
            Voyant_Recherche_Place.Visibility = Visibility.Visible;
        }
        // =========================================================
        // 🔥 VALIDATION MOTEUR
        // =========================================================
        private bool bAbsenceTable = false;
        private DateTime HeureAbsence;

        // =========================================================
        // AUCUNE TABLE
        // Retourne true si aucune table n'est présente
        // =========================================================
        private bool Aucune_Table()
        {
            return Lire_Aucune_Table();
        }

        // =========================================================
        // ABSENCE TABLE
        // Retourne true si la table est absente depuis plus de 7.5 s
        // =========================================================
        private bool Absence_Table()
        {
            // La table est présente
            if (!Aucune_Table())
            {
                Voyant_Validation_Moteur.Background = Brushes.LimeGreen;

                if (bAbsenceTable)
                {
                    bAbsenceTable = false;
                }

                return false;
            }

            // Première détection
            if (!bAbsenceTable)
            {
                bAbsenceTable = true;
                HeureAbsence = DateTime.Now;

                Voyant_Validation_Moteur.Background = Brushes.Red;

                return false;
            }

            // Timeout atteint ?
            if ((DateTime.Now - HeureAbsence).TotalSeconds >= 7.5)
            {
                Voyant_Validation_Moteur.Background = Brushes.Red;

                bAbsenceTable = false;

                return true;
            }

            return false;
        }

        // =========================================================
        // LIRE AUCUNE TABLE
        // =========================================================
        private bool Lire_Aucune_Table()
        {
            for (int i = 0; i < AucuneTableDetect.Length; i++)
            {
                int couleur = LirePixel(
                    AucuneTableDetect[i].X,
                    AucuneTableDetect[i].Y);
                if (couleur == AucuneTableDetect[i].Couleur)
                    return false;
            }

            return true;
        }
        private string Action_Mode_Actif()
        {
            if (bTriggersBoard_Actif)
                return "PAROLE";

            if (bTapisPreFlop_Actif)
                return "PAROLE"; //Action_Tapis();

            if (bIA_Actif)
                return "PAROLE"; // Action_IA();

            return "PAROLE";
        }
        private void StopSuperviseur()
        {
            bSuperviseurVert_Actif = false;

            bAutoriserHistorique = false;

            Reset_Table();
        }
        private void ResetPosition()
        {
            foreach (var b in BoutonsPosition)
                b.Background = (Brush)FindResource("blanc");
        }

        private void ResetNiveauTable()
        {
            foreach (var b in BoutonsNiveauTable)
                b.Background = (Brush)FindResource("blanc");
        }
        private void ActiverPosition(Button boutonActif)
        {
            ResetPosition();
            boutonActif.Background = (Brush)FindResource("bleu");
        }

        private void ActiverNiveauTable(Button boutonActif)
        {
            ResetNiveauTable();
            boutonActif.Background = (Brush)FindResource("bleu");
        }

        // ---------------------------------------------------------
        // Validation_Moteur()
        // ---------------------------------------------------------

        #endregion

        // =========================================================
        // 🔥 STOP TOUS MODES
        // =========================================================
        private void StopTousModes()
        {
            // Sauvegarder uniquement s'il y a un historique
            if (RTB_Historique.Document.Blocks.Count > 0)
            {
                Sauvegarder_Historique_TXT();
                Sauvegarder_Historique_RTF();
                Sauvegarder_Historique_JSON();
            }

            bAutoriserHistorique = false;

            bTestBoard_Actif = false;
            bTriggersBoard_Actif = false;
            bTapisPreFlop_Actif = false;
            bIA_Actif = false;

            Bt_Test_Board.Background = Brushes.White;
            Bt_Triggers_Board.Background = Brushes.White;
            Bt_Tapis_PreFlop.Background = Brushes.White;
            Bt_IA.Background = Brushes.White;

            Reset_Table();
        }

        // =========================================================
        // 🔥 FENÊTRE CHARGÉE
        // =========================================================
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Superviseur_Poker();
        }
        #region BOUTON DEBUG
        // =========================================================
        // 🔥 BOUTON DEBUG
        // =========================================================
        private void Bt_Debug_Click(object sender, RoutedEventArgs e)
        {

            bDebug = !bDebug;

            if (bDebug)
                Mode_Debug_ON();
            else
                Mode_Debug_OFF();
        }

        private void Mode_Debug_OFF()
        {
            Bt_Debug.Background = Brushes.White;

            //RTB Agrandissement
            RTB_Historique.Margin = new Thickness(136, 350, 0, 334);

            // Supprimer les points si besoin
            if (!bPoints)
                Bt_Points_Click(null, null);

            // Le bouton Points disparaît
            Bt_Points.Visibility = Visibility.Collapsed;

            // Zone Debug masquée
            TB_Debug.Visibility = Visibility.Collapsed;

            Bt_Injection.Visibility = Visibility.Collapsed;

            Bt_Test_Board.Visibility = Visibility.Collapsed;

            // Tirages Aléatoires
            Bt_Rand_M.Visibility = Visibility.Collapsed;
            Bt_Rand_F.Visibility = Visibility.Collapsed;
            Bt_Rand_T.Visibility = Visibility.Collapsed;
            Bt_Rand_R.Visibility = Visibility.Collapsed;
            Bt_Rand_MTFR.Visibility = Visibility.Collapsed;

            Bt_Je_suis_a_table.Visibility = Visibility.Collapsed;
            Bt_Pas_de_Bouton.Visibility = Visibility.Collapsed;

            // Joueurs présents
            Bt_Un_Joueur_en_1.Visibility = Visibility.Collapsed;
            Bt_Un_Joueur_en_2.Visibility = Visibility.Collapsed;
            Bt_Un_Joueur_en_3.Visibility = Visibility.Collapsed;
            Bt_Un_Joueur_en_4.Visibility = Visibility.Collapsed;
            Bt_Un_Joueur_en_5.Visibility = Visibility.Collapsed;
            Bt_Un_Joueur_en_6.Visibility = Visibility.Collapsed;
            Bt_Un_Joueur_en_7.Visibility = Visibility.Collapsed;
            Bt_Un_Joueur_en_8.Visibility = Visibility.Collapsed;
            Bt_Un_Joueur_en_9.Visibility = Visibility.Collapsed;

            // Bouton Dealer
            Bt_Bouton_en_1.Visibility = Visibility.Collapsed;
            Bt_Bouton_en_2.Visibility = Visibility.Collapsed;
            Bt_Bouton_en_3.Visibility = Visibility.Collapsed;
            Bt_Bouton_en_4.Visibility = Visibility.Collapsed;
            Bt_Bouton_en_5.Visibility = Visibility.Collapsed;
            Bt_Bouton_en_6.Visibility = Visibility.Collapsed;
            Bt_Bouton_en_7.Visibility = Visibility.Collapsed;
            Bt_Bouton_en_8.Visibility = Visibility.Collapsed;
            Bt_Bouton_en_9.Visibility = Visibility.Collapsed;

            // Places disponibles
            Bt_PlaceDisponible_1.Visibility = Visibility.Collapsed;
            Bt_PlaceDisponible_2.Visibility = Visibility.Collapsed;
            Bt_PlaceDisponible_3.Visibility = Visibility.Collapsed;
            Bt_PlaceDisponible_4.Visibility = Visibility.Collapsed;
            Bt_PlaceDisponible_5.Visibility = Visibility.Collapsed;
            Bt_PlaceDisponible_6.Visibility = Visibility.Collapsed;
            Bt_PlaceDisponible_7.Visibility = Visibility.Collapsed;
            Bt_PlaceDisponible_8.Visibility = Visibility.Collapsed;
            Bt_PlaceDisponible_9.Visibility = Visibility.Collapsed;

            // Fold
            Bt_Joueur1_Fold.Visibility = Visibility.Collapsed;
            Bt_Joueur2_Fold.Visibility = Visibility.Collapsed;
            Bt_Joueur3_Fold.Visibility = Visibility.Collapsed;
            Bt_Joueur4_Fold.Visibility = Visibility.Collapsed;
            Bt_Joueur5_Fold.Visibility = Visibility.Collapsed;
            Bt_Joueur6_Fold.Visibility = Visibility.Collapsed;
            Bt_Joueur7_Fold.Visibility = Visibility.Collapsed;
            Bt_Joueur8_Fold.Visibility = Visibility.Collapsed;
            Bt_Joueur9_Fold.Visibility = Visibility.Collapsed;

            // Mise
            Bt_Joueur1_Mise.Visibility = Visibility.Collapsed;
            Bt_Joueur2_Mise.Visibility = Visibility.Collapsed;
            Bt_Joueur3_Mise.Visibility = Visibility.Collapsed;
            Bt_Joueur4_Mise.Visibility = Visibility.Collapsed;
            Bt_Joueur5_Mise.Visibility = Visibility.Collapsed;
            Bt_Joueur6_Mise.Visibility = Visibility.Collapsed;
            Bt_Joueur7_Mise.Visibility = Visibility.Collapsed;
            Bt_Joueur8_Mise.Visibility = Visibility.Collapsed;
            Bt_Joueur9_Mise.Visibility = Visibility.Collapsed;

            // Tapis
            Bt_Joueur1_Tapis.Visibility = Visibility.Collapsed;
            Bt_Joueur2_Tapis.Visibility = Visibility.Collapsed;
            Bt_Joueur3_Tapis.Visibility = Visibility.Collapsed;
            Bt_Joueur4_Tapis.Visibility = Visibility.Collapsed;
            Bt_Joueur5_Tapis.Visibility = Visibility.Collapsed;
            Bt_Joueur6_Tapis.Visibility = Visibility.Collapsed;
            Bt_Joueur7_Tapis.Visibility = Visibility.Collapsed;
            Bt_Joueur8_Tapis.Visibility = Visibility.Collapsed;
            Bt_Joueur9_Tapis.Visibility = Visibility.Collapsed;

            // Stack départ
            T_Stak_Depart_Joueur1.Visibility = Visibility.Collapsed;
            T_Stak_Depart_Joueur2.Visibility = Visibility.Collapsed;
            T_Stak_Depart_Joueur3.Visibility = Visibility.Collapsed;
            T_Stak_Depart_Joueur4.Visibility = Visibility.Collapsed;
            T_Stak_Depart_Joueur5.Visibility = Visibility.Collapsed;
            T_Stak_Depart_Joueur6.Visibility = Visibility.Collapsed;
            T_Stak_Depart_Joueur7.Visibility = Visibility.Collapsed;
            T_Stak_Depart_Joueur8.Visibility = Visibility.Collapsed;
            T_Stak_Depart_Joueur9.Visibility = Visibility.Collapsed;

            // Gain
            Bt_Gagne_en_1.Visibility = Visibility.Collapsed;
            Bt_Gagne_en_2.Visibility = Visibility.Collapsed;
            Bt_Gagne_en_3.Visibility = Visibility.Collapsed;
            Bt_Gagne_en_4.Visibility = Visibility.Collapsed;
            Bt_Gagne_en_5.Visibility = Visibility.Collapsed;
            Bt_Gagne_en_6.Visibility = Visibility.Collapsed;
            Bt_Gagne_en_7.Visibility = Visibility.Collapsed;
            Bt_Gagne_en_8.Visibility = Visibility.Collapsed;
            Bt_Gagne_en_9.Visibility = Visibility.Collapsed;
        }

        private void Mode_Debug_ON()
        {
            Bt_Debug.Background = Brushes.LightGreen;

            //RTB Réduction
            RTB_Historique.Margin = new Thickness(136, 462, 0, 334);

            // Remettre les points si besoin
            if (bPoints)
                Bt_Points_Click(null, null);

            // Le bouton Points est visible et vert
            Bt_Points.Visibility = Visibility.Visible;
            Bt_Points.Background = Brushes.LightGreen;

            // Zone Debug
            TB_Debug.Visibility = Visibility.Visible;

            Bt_Injection.Visibility = Visibility.Visible;

            Bt_Test_Board.Visibility = Visibility.Visible;

            Bt_Rand_M.Visibility = Visibility.Visible;
            Bt_Rand_F.Visibility = Visibility.Visible;
            Bt_Rand_T.Visibility = Visibility.Visible;
            Bt_Rand_R.Visibility = Visibility.Visible;
            Bt_Rand_MTFR.Visibility = Visibility.Visible;

            Bt_Je_suis_a_table.Visibility = Visibility.Visible;
            Bt_Pas_de_Bouton.Visibility = Visibility.Visible;

            // Joueurs présents
            Bt_Un_Joueur_en_1.Visibility = Visibility.Visible;
            Bt_Un_Joueur_en_2.Visibility = Visibility.Visible;
            Bt_Un_Joueur_en_3.Visibility = Visibility.Visible;
            Bt_Un_Joueur_en_4.Visibility = Visibility.Visible;
            Bt_Un_Joueur_en_5.Visibility = Visibility.Visible;
            Bt_Un_Joueur_en_6.Visibility = Visibility.Visible;
            Bt_Un_Joueur_en_7.Visibility = Visibility.Visible;
            Bt_Un_Joueur_en_8.Visibility = Visibility.Visible;
            Bt_Un_Joueur_en_9.Visibility = Visibility.Visible;

            // Bouton Dealer
            Bt_Bouton_en_1.Visibility = Visibility.Visible;
            Bt_Bouton_en_2.Visibility = Visibility.Visible;
            Bt_Bouton_en_3.Visibility = Visibility.Visible;
            Bt_Bouton_en_4.Visibility = Visibility.Visible;
            Bt_Bouton_en_5.Visibility = Visibility.Visible;
            Bt_Bouton_en_6.Visibility = Visibility.Visible;
            Bt_Bouton_en_7.Visibility = Visibility.Visible;
            Bt_Bouton_en_8.Visibility = Visibility.Visible;
            Bt_Bouton_en_9.Visibility = Visibility.Visible;

            // Places disponibles
            Bt_PlaceDisponible_1.Visibility = Visibility.Visible;
            Bt_PlaceDisponible_2.Visibility = Visibility.Visible;
            Bt_PlaceDisponible_3.Visibility = Visibility.Visible;
            Bt_PlaceDisponible_4.Visibility = Visibility.Visible;
            Bt_PlaceDisponible_5.Visibility = Visibility.Visible;
            Bt_PlaceDisponible_6.Visibility = Visibility.Visible;
            Bt_PlaceDisponible_7.Visibility = Visibility.Visible;
            Bt_PlaceDisponible_8.Visibility = Visibility.Visible;
            Bt_PlaceDisponible_9.Visibility = Visibility.Visible;

            // Fold
            Bt_Joueur1_Fold.Visibility = Visibility.Visible;
            Bt_Joueur2_Fold.Visibility = Visibility.Visible;
            Bt_Joueur3_Fold.Visibility = Visibility.Visible;
            Bt_Joueur4_Fold.Visibility = Visibility.Visible;
            Bt_Joueur5_Fold.Visibility = Visibility.Visible;
            Bt_Joueur6_Fold.Visibility = Visibility.Visible;
            Bt_Joueur7_Fold.Visibility = Visibility.Visible;
            Bt_Joueur8_Fold.Visibility = Visibility.Visible;
            Bt_Joueur9_Fold.Visibility = Visibility.Visible;

            // Mise
            Bt_Joueur1_Mise.Visibility = Visibility.Visible;
            Bt_Joueur2_Mise.Visibility = Visibility.Visible;
            Bt_Joueur3_Mise.Visibility = Visibility.Visible;
            Bt_Joueur4_Mise.Visibility = Visibility.Visible;
            Bt_Joueur5_Mise.Visibility = Visibility.Visible;
            Bt_Joueur6_Mise.Visibility = Visibility.Visible;
            Bt_Joueur7_Mise.Visibility = Visibility.Visible;
            Bt_Joueur8_Mise.Visibility = Visibility.Visible;
            Bt_Joueur9_Mise.Visibility = Visibility.Visible;

            // Tapis
            Bt_Joueur1_Tapis.Visibility = Visibility.Visible;
            Bt_Joueur2_Tapis.Visibility = Visibility.Visible;
            Bt_Joueur3_Tapis.Visibility = Visibility.Visible;
            Bt_Joueur4_Tapis.Visibility = Visibility.Visible;
            Bt_Joueur5_Tapis.Visibility = Visibility.Visible;
            Bt_Joueur6_Tapis.Visibility = Visibility.Visible;
            Bt_Joueur7_Tapis.Visibility = Visibility.Visible;
            Bt_Joueur8_Tapis.Visibility = Visibility.Visible;
            Bt_Joueur9_Tapis.Visibility = Visibility.Visible;

            // Stack départ
            T_Stak_Depart_Joueur1.Visibility = Visibility.Visible;
            T_Stak_Depart_Joueur2.Visibility = Visibility.Visible;
            T_Stak_Depart_Joueur3.Visibility = Visibility.Visible;
            T_Stak_Depart_Joueur4.Visibility = Visibility.Visible;
            T_Stak_Depart_Joueur5.Visibility = Visibility.Visible;
            T_Stak_Depart_Joueur6.Visibility = Visibility.Visible;
            T_Stak_Depart_Joueur7.Visibility = Visibility.Visible;
            T_Stak_Depart_Joueur8.Visibility = Visibility.Visible;
            T_Stak_Depart_Joueur9.Visibility = Visibility.Visible;

            // Gain
            Bt_Gagne_en_1.Visibility = Visibility.Visible;
            Bt_Gagne_en_2.Visibility = Visibility.Visible;
            Bt_Gagne_en_3.Visibility = Visibility.Visible;
            Bt_Gagne_en_4.Visibility = Visibility.Visible;
            Bt_Gagne_en_5.Visibility = Visibility.Visible;
            Bt_Gagne_en_6.Visibility = Visibility.Visible;
            Bt_Gagne_en_7.Visibility = Visibility.Visible;
            Bt_Gagne_en_8.Visibility = Visibility.Visible;
            Bt_Gagne_en_9.Visibility = Visibility.Visible;
        }
        #endregion

        // =========================================================
        // 🛑 ARRÊT D'URGENCE
        // =========================================================
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control &&
                e.Key == Key.F12)
            {
                bSuperviseurActif = false;
                //Close();
            }
        }

        // =========================================================
        // FERMER
        // =========================================================
        private void Fermer_Click(object sender, RoutedEventArgs e)
        {
            StopTousModes();

            Sauver_Preferences_Utilisateur();

            // =====================================
            // 🔥 SAUVEGARDE DES 3 RANGES
            // =====================================
            Sauver_Range_Tapis();
            Sauver_Range_Relance();
            Sauver_Range_Suivre();

            Close();
        }

        // =========================================================
        // 🔥 DEBUG JEU
        // =========================================================
        private void Debug_Jeu(string phase, Jeu jeu)
        {
            string combinaison;

            // =====================================
            // PREFLOP
            // =====================================
            if (phase == "PRE FLOP")
            {
                if (bPreFlop_Paire && bPreFlop_Pareille)
                    combinaison = "Paire Pareillée";
                else if (bPreFlop_Paire)
                    combinaison = "Paire";
                else if (bPreFlop_Pareille && bPreFlop_Suite)
                    combinaison = "Suite Pareillée";
                else if (bPreFlop_Pareille)
                    combinaison = "Pareillée";
                else if (bPreFlop_Suite)
                    combinaison = "Suite";
                else
                    combinaison = "Rien";

                return;
            }

            // =====================================
            // FLOP / TURN / RIVER
            // =====================================
            switch (jeu.Force)
            {
                case 0:
                    combinaison = "Rien";
                    break;

                case 1:
                    combinaison = "Paire";
                    break;

                case 2:
                    combinaison = "Deux Paires";
                    break;

                case 3:
                    combinaison = "Brelan";
                    break;

                case 4:
                    combinaison = $"Quinte {NomCarte(jeu.Qualite)} haute";
                    break;

                case 5:
                    combinaison = $"Couleur {NomCarte(jeu.Qualite)} haute";
                    break;

                case 6:
                    combinaison = "Full";
                    break;

                case 7:
                    combinaison = "Carré";
                    break;

                case 8:
                    combinaison = $"Quinte Flush {NomCarte(jeu.Qualite)} haute";
                    break;

                default:
                    combinaison = "?";
                    break;
            }

        }


        // =========================================================
        // DEBUG
        // =========================================================
        private void Debug(string message)
        {
            // Effacer au début d'une nouvelle lecture
            if (message == "Lecture Main1")
                TB_Debug.Clear();

            TB_Debug.AppendText(message + Environment.NewLine);
            TB_Debug.ScrollToEnd();
        }

        private class EtatRange
        {
            public int Memo { get; set; }
            public List<string> Range { get; set; } = new List<string>();
        }
    }
}
