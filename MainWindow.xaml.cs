using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
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
using System.Runtime.InteropServices;
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


    public partial class MainWindow : Window
    {
        // =========================================================
        // API WINDOWS SOURIS
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

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        private int Nb_Mains_Testees = 0;

        // STRUCTURE DETECTION CARTE
        public struct S_CarteDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION JOUEUR A TAPIS
        public struct S_JoueurATapisDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION JOUEUR REFLECHIT
        public struct S_JoueurReflechitDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION JOUEUR RELANCE
        public struct S_JoueurRelanceDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION JOUEUR SE COUCHE
        public struct S_JoueurSeCoucheDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION JOUEUR SUIT
        public struct S_JoueurSuitDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION MA POSITION
        public struct S_MaPositionDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }
        
        // STRUCTURE DETECTION PLACES LIBRES
        public struct S_PlacesLibresDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION NIVEAU TABLE
        public struct S_NiveauTableDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION POSITION BOUTON
        public struct S_PositionBoutonDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION PUBLICITE
        public struct S_PubliciteDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION PLUS DE JETONS
        public struct S_PlusDeJetonsDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION FELICITATION
        public struct S_FelicitationDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION NOUVEAU NIVEAU
        public struct S_NouveauNiveauDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION JOUEURPAROLE
        public struct S_JoueurParoleDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION PLUS DE POT
        public struct S_PlusDePotDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }

        // STRUCTURE DETECTION PUPITRE GRIS
        public struct S_PupitreGrisDetect
        {
            public string Nom;
            public int X;
            public int Y;
            public int Couleur;
        }





        private const string FichierHistoriqueTXT = @"F:\source\repos\Poker\Historique.txt";

        private const string FichierHistoriqueRTF = @"F:\source\repos\Poker\Historique.rtf";

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

        // TABLEAU DETECTION EVENEMENTIELLE
        private S_CarteDetect[] Main_BoardDetect = new S_CarteDetect[83];

        // TABLEAU DETECTION MA POSITION
        private S_MaPositionDetect[] MaPositionDetect = new S_MaPositionDetect[9]; 

        // TABLEAU DETECTION PLACE LIBRE
        private S_PlacesLibresDetect[] PlacesLibresDetect = new S_PlacesLibresDetect[9];

        // TABLEAU DETECTION NIVEAU TABLE
        private S_NiveauTableDetect[] NiveauTableDetect = new S_NiveauTableDetect[12];

        // TABLEAU DETECTION POSITION BOUTON
        private S_PositionBoutonDetect[] PositionBoutonDetect = new S_PositionBoutonDetect[9];

        // TABLEAU DETECTION PUBLCITE
        private S_PubliciteDetect[] PubliciteDetect = new S_PubliciteDetect[1];

        // TABLEAU DETECTION PLUS DE JETONS
        private S_PlusDeJetonsDetect[] PlusDeJetonsDetect = new S_PlusDeJetonsDetect[1];

        // TABLEAU DETECTION FELICITATION
        private S_FelicitationDetect[] FelicitationDetect = new S_FelicitationDetect[1];

        // TABLEAU DETECTION NOUVEAU NIVEAU
        private S_NouveauNiveauDetect[] NouveauNiveauDetect = new S_NouveauNiveauDetect[1];

        // TABLEAU DETECTION PLUS DE POT
        private S_PlusDePotDetect[] PlusDePotDetect = new S_PlusDePotDetect[1];

        // TABLEAU DETECTION PUPITRE GRIS
        private S_PupitreGrisDetect[] PupitreGrisDetect = new S_PupitreGrisDetect[2];



        // TABLEAU DETECTION JOUEUR PAROLE
        private S_JoueurParoleDetect[] JoueurParoleDetect = new S_JoueurParoleDetect[9];

        // TABLEAU DETECTION JOUEUR SE COUCHE
        private S_JoueurSeCoucheDetect[] JoueurSeCoucheDetect = new S_JoueurSeCoucheDetect[9];

        // TABLEAU DETECTION JOUEUR Suit
        private S_JoueurSuitDetect[] JoueurSuitDetect = new S_JoueurSuitDetect[9];

        // TABLEAU DETECTION JOUEUR RELANCE
        private S_JoueurRelanceDetect[] JoueurRelanceDetect = new S_JoueurRelanceDetect[9];

        // TABLEAU DETECTION JOUEUR REFLECHIT
        private S_JoueurReflechitDetect[] JoueurReflechitDetect = new S_JoueurReflechitDetect[9];

        // TABLEAU DETECTION JOUEUR A TAPIS
        private S_JoueurATapisDetect[] JoueurATapisDetect = new S_JoueurATapisDetect[9];

        // 752 pixels injectés 

        

        // =========================================================
        // MISE SELECTIONNEE
        // =========================================================
        private int MiseSelectionnee = 0;

        // =====================================================
        // PREMIERE FUSION HISTORIQUE
        // =====================================================
        private bool bPremiereFusionHistorique = true;

        private bool bPremiereMain = true;

        private int Signature_Main1 = 0;
        private int Signature_Main2 = 0;

        private string Carte, Memo_Max_Suit, Main1, Main2, Flop1, Flop2, Flop3, Turn, River, M, M1_Val, M2_Val;
        private string ActionForcee = null;
        private float flottant;
        private int entier;
        DateTime DebutSession;
        DispatcherTimer timer;
        int NbMains = 0;
        int NbTapis = 0;
        int NbParole = 0;
        bool JeuEnPause = false;
        DateTime DebutPause;

        private bool bTir_Flop_Quinte, bTir_Flop_Couleur, bTir_Flop_QFlush;
        private bool bTir_Turn_Quinte, bTir_Turn_Couleur, bTir_Turn_QFlush;
        private bool bPreFlop_Paire, bPreFlop_Suite, bPreFlop_Pareille;
        private bool bFlop_Paire, bFlop_2_Paires, bFlop_Brelan, bFlop_Quinte, bFlop_Couleur, bFlop_Full, bFlop_Carre, bFlop_QFlush;
        private bool bTurn_Paire, bTurn_2_Paires, bTurn_Brelan, bTurn_Quinte, bTurn_Couleur, bTurn_Full, bTurn_Carre, bTurn_QFlush;
        private bool bRiver_Paire, bRiver_2_Paires, bRiver_Brelan, bRiver_Quinte, bRiver_Couleur, bRiver_Full, bRiver_Carre, bRiver_QFlush;

        private bool bSe_Recare;
        private bool NM9, NM8, NM6, NM2, NM0, NP2, NP4, NP6, NP8, NP10;
        private bool bPosition_1, bPosition_2, bPosition_3, bPosition_4, bPosition_5, bPosition_6, bPosition_7, bPosition_8, bPosition_9;
        private bool bGrenhorn, bBeginner, bGambler, bPro, bBigFish, bMaster, bChampion, bGuru, bUltimate, bEpic, bInsane, bLegendary;

        private bool ss, bBouton_en_1, bBouton_en_2, bBouton_en_3, bBouton_en_4, bBouton_en_5, bBouton_en_6, bBouton_en_7, bBouton_en_8, bBouton_en_9;

        private int NbDoublon_Flop1;
        private int NbDoublon_Flop2;
        private int NbDoublon_Flop3;

        private int NbDoublon_Turn;
        private int NbDoublon_River;
        //----------------------------------------------------------
        private bool bTriggersBoard = false; 
        private bool bTriggersBoard_Actif = false;

        private bool bTapisPreFlop = false;
        private bool bTapisPreFlop_Actif = false;

        private bool bTestBoard = false;
        private bool bTestBoard_Actif = false;
        //----------------------------------------------------------

        private bool bPoints = true;

        private System.Drawing.Bitmap Bitmap_Main;

        private string Main_Board_Presence = "";

        private int NbTurnNormal = 0;
        private int NbTurnHB = 0;

        private int NbRiverNormal = 0;
        private int NbRiverHB = 0;

        private int Sig_M1_X = 1;
        private int Sig_M1_Y = 1;
        private int Sig_M1_C = 0;

        private int Sig_M2_X = 1;
        private int Sig_M2_Y = 1;
        private int Sig_M2_C = 0;

        // =========================================================
        // AUTORISATION D'ECRITURE DANS L'HISTORIQUE
        // =========================================================
        private bool bAutoriserHistorique = true;

        private const string DOSSIER_JSON = @"F:\source\repos\Poker\JSON";
        private static readonly string FichierHistoriqueJSON = Path.Combine(DOSSIER_JSON, "Historique.json");
        private List<MainHistorique> Historique = new List<MainHistorique>();
        private bool bArretDemande = false;

        // Délai entre l'appui et le relâchement du clic (ms)
        private int DelaiClic = 0;

        HashSet<string> RangeTapis = new HashSet<string>();
        HashSet<string> RangeRelance = new HashSet<string>();
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

        int niveauMise = 0;
        int NbTapisGagne = 0;
        bool autoActif = false;
        List<Button> NiveauxMise = new List<Button>();
        bool premierGO = true;
        bool niveauInitialise = false;
        DispatcherTimer timerGoFlash;
        bool flashGO = false;
        int compteurFlashGO = 0;

        // Carte sélectionnée manuellement (clic sur la grille 52 cartes)
        private string CarteCourante = "";

        // Indique si le paquet a été initialisé
        private bool PaquetInitialise = false;

        private readonly string[] Valeurs = {"A","K","Q","J","10","9","8","7","6","5","4","3","2"};

        private readonly string[] Symboles = {"♣","♦","♥","♠"};

        // Générateur aléatoire
        Random rnd = new Random();

        // Paquet de cartes (0..51)
        List<int> Paquet = new List<int>();

        public MainWindow()
        {
            
            
            InitializeComponent();

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

            Charger_Main_Board_JSON();

            Charger_Ma_Position_JSON();
            Charger_Places_Libres_JSON();
            Charger_Niveau_Table_JSON();
            Charger_Position_Bouton_JSON();
            Charger_Publicite_JSON();
            Charger_Pupitre_Gris_JSON();
            Charger_Plus_De_Jetons_JSON();
            Charger_Plus_De_Pot_JSON();
            Charger_Felicitation_JSON();
            Charger_Joueur_Parole_JSON();

            Charger_Joueur_Se_Couche_JSON();
            Charger_Joueur_Suit_JSON();
            Charger_Joueur_Relance_JSON();
            Charger_Joueur_Reflechit_JSON();
            Charger_Joueur_A_Tapis_JSON();

            // 🔥 Variables
            DebutSession = DateTime.Now;

            // 🔥 Timer principal
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            // 🔥 Gestion pause automatique (réduction / reprise)
            this.StateChanged += Window_StateChanged;

            for (int i = 0; i <= 12; i++)
            {
                CbRandom.Items.Add(i);
            }

            CbRandom.SelectedIndex = 0;
            MettreAJourIndicateurRandom();

            Orange = (Brush)FindResource("orange");
            Blanc = (Brush)FindResource("blanc");
            Bleu = (Brush)FindResource("bleu");
            Vert = (Brush)FindResource("vert");
            Gold = (Brush)FindResource("gold");

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

            // 👉 Appliquer Memo07 au démarrage
            //Bt_Mem07_Click(null, null);

            if (Bt_Se_Recare.Background.Equals((Brush)FindResource("blanc")))
            {
                Bt_Se_Recare_Click(null, null);
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

            if (niveau == 0)
                return;

            int indexRandom;

            if (niveau == 1)
            {
                // Memo01 (index 0) ou Memo13 (index 12)
                int[] choix = { 0, 12 };
                indexRandom = choix[rnd.Next(2)];
            }
            else
            {
                indexRandom = rnd.Next(niveau);
            }

            BoutonsMemo[indexRandom].RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent)
            );
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

        // Convertit "10♠" ou "A♥" en données moteur exploitables
        private CarteInfo ExtraireCarte(string carte)
        {
            string[] valeurs = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
            string couleurs = "♣♦♥♠";

            // séparer valeur / couleur
            string val = carte.Substring(0, carte.Length - 1);
            char coul = carte[carte.Length - 1];


            int iv = Array.IndexOf(valeurs, val);
            int ic = couleurs.IndexOf(coul);

            return new CarteInfo
            {
                Valeur = iv + 1,      // 1..13
                Couleur = ic,         // 0..3
                Numero = iv * 4 + ic  // 0..51
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
            if (string.IsNullOrEmpty(CarteCourante)) return;

            // contrôle ordre via couleur
            if (precedent != null && precedent.Background != Orange)
                return;

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
            if(!bTestBoard_Actif)
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
                LbRandomStatus.Background = new SolidColorBrush(Color.FromRgb(230, 230, 230));
                LbRandomStatus.Foreground = Brushes.Black;
                return;
            }

            LbRandomStatus.Content = $"● RANDOM {niveau}";

            if (niveau <= 4)
            {
                LbRandomStatus.Background = new SolidColorBrush(Color.FromRgb(120, 200, 120));
                LbRandomStatus.Foreground = Brushes.Black;
            }
            else if (niveau <= 8)
            {
                LbRandomStatus.Background = new SolidColorBrush(Color.FromRgb(255, 170, 60));
                LbRandomStatus.Foreground = Brushes.Black;
            }
            else
            {
                LbRandomStatus.Background = new SolidColorBrush(Color.FromRgb(210, 70, 70));
                LbRandomStatus.Foreground = Brushes.White;
            }
        }
        /************************************
        **                                 **
        **              M A I N            **
        **                                 **
        ************************************/

        private void Bt_Main_1_Click(object sender, RoutedEventArgs e)
        {
            // =========================================================
            // 🔥 RESET TIRAGES
            // À mettre dans Bt_Main_1_Click()
            // =========================================================
            Bt_Doublon.Background = Blanc;

            bTir_Flop_Quinte = false;
            bTir_Flop_Couleur = false;
            bTir_Flop_QFlush = false;

            bTir_Turn_Quinte = false;
            bTir_Turn_Couleur = false;
            bTir_Turn_QFlush = false;

            Bt_Tir_Flop_Quinte.Background = Blanc;
            Bt_Tir_Flop_Couleur.Background = Blanc;
            Bt_Tir_Flop_QFlush.Background = Blanc;

            Bt_Tir_Turn_Quinte.Background = Blanc;
            Bt_Tir_Turn_Couleur.Background = Blanc;
            Bt_Tir_Turn_QFlush.Background = Blanc;
            NbMains++;


            AppliquerRandomSiActif();

            InitialiserPaquet();
            //Efface_Affichage_Cartes();
            //Initialise_Boutons_Main_Flop_Turn_River();

            // 🔥 RESET boutons ACTION
            Bt_Tapis.Background = Blanc;
            Bt_J_ai_Gagne.Background = (Brush)FindResource("blanc");

            // 🔥 RESET booleans PréFlop Flop Turn River
            bPreFlop_Paire = false;
            bPreFlop_Pareille = false;
            bPreFlop_Suite = false;

            bFlop_Paire = false;
            bFlop_2_Paires = false;
            bFlop_Brelan = false;
            bFlop_Quinte = false;
            bFlop_Couleur = false;
            bFlop_Full = false;
            bFlop_Carre = false;
            bFlop_QFlush = false;

            bTurn_Paire = false;
            bTurn_2_Paires = false;
            bTurn_Brelan = false;
            bTurn_Quinte = false;
            bTurn_Couleur = false;
            bTurn_Full = false;
            bTurn_Carre = false;
            bTurn_QFlush = false;

            bRiver_Paire = false;
            bRiver_2_Paires = false;
            bRiver_Brelan = false;
            bRiver_Quinte = false;
            bRiver_Couleur = false;
            bRiver_Full = false;
            bRiver_Carre = false;
            bRiver_QFlush = false;




            // 🔥 RESET affichage boutons PréFlop Flop Turn River
            Bt_PreFlop_Paire.Background = Blanc;
            Bt_PreFlop_Pareille.Background = Blanc;

            Bt_Flop_Paire.Background = Blanc;
            Bt_Flop_2_Paires.Background = Blanc;
            Bt_Flop_Brelan.Background = Blanc;
            Bt_Flop_Quinte.Background = Blanc;
            Bt_Flop_Couleur.Background = Blanc;
            Bt_Flop_Full.Background = Blanc;
            Bt_Flop_Carre.Background = Blanc;
            Bt_Flop_QFlush.Background = Blanc;


            Bt_Turn_Paire.Background = Blanc;
            Bt_Turn_2_Paires.Background = Blanc;
            Bt_Turn_Brelan.Background = Blanc;
            Bt_Turn_Quinte.Background = Blanc;
            Bt_Turn_Couleur.Background = Blanc;
            Bt_Turn_Full.Background = Blanc;
            Bt_Turn_Carre.Background = Blanc;
            Bt_Turn_QFlush.Background = Blanc;

            Bt_River_Paire.Background = Blanc;
            Bt_River_2_Paires.Background = Blanc;
            Bt_River_Brelan.Background = Blanc;
            Bt_River_Quinte.Background = Blanc;
            Bt_River_Couleur.Background = Blanc;
            Bt_River_Full.Background = Blanc;
            Bt_River_Carre.Background = Blanc;
            Bt_River_QFlush.Background = Blanc;


            AfficherCarte(TB_M1, null, Bt_Main_1, 0);
        }

        private void Bt_Main_2_Click(object sender, RoutedEventArgs e)
        {
            NbMains++;

            AfficherCarte(TB_M2, Bt_Main_1, Bt_Main_2, 1);

            // 🔥 Lecture brute des cartes
            string c1 = TB_M1.Text;
            string c2 = TB_M2.Text;

            // 🔒 Sécurité cartes identiques
            if (!string.IsNullOrEmpty(c1) && c1 == c2)
            {
                Bt_Parole_Passe.Background = Vert;
                NbParole++;
                return;
            }

            // 🔥 Lecture de la main
            string main = GetMain();

            // 🔒 Sécurité lecture vide
            if (string.IsNullOrWhiteSpace(main))
            {
                Bt_Parole_Passe.Background = Vert;
                NbParole++;
                return;
            }

            // 🔥 Normalisation
            main = main.Trim();

            // =========================================================
            // 🔥 CALCUL PRE-FLOP
            // =========================================================

            Calcul_PreFlop();

            // =========================================================
            // 🔥 ACTION FORCÉE
            // =========================================================

            if (ActionForcee != null)
            {
                AppliquerActionForcee();

                string action = ActionForcee.ToUpper();

                if (action == "TAPIS")
                    NbTapis++;
                else
                    NbParole++;

                ActionForcee = null;

                return;
            }

            // 🔄 Reset boutons action
            Bt_Tapis.Background = Blanc;
            Bt_Parole_Passe.Background = Blanc;
            Bt_Call.Background = Blanc;
            Bt_Raise.Background = Blanc;

            // 🔒 Sécurité range
            if (RangeTapis == null || RangeTapis.Count == 0)
            {
                Bt_Parole_Passe.Background = Vert;
                NbParole++;
                return;
            }

            // 🎯 Décision
            if (EstDansRange(main))
            {
                Bt_Tapis.Background = Vert;
                NbTapis++;

                // 🔥 Gestion limite tapis (0 = illimité)
                if (int.TryParse(Tb_Tapis_Max.Text, out int maxTapis)
                    && maxTapis > 0
                    && NbTapis >= maxTapis)
                {
                    JeuEnPause = true;

                    DebutPause = DateTime.Now;

                    timer.Stop();

                    this.WindowState = WindowState.Minimized;

                    Console.WriteLine("PAUSE AUTO (TAPIS MAX)");

                    return;
                }
            }
            else
            {
                Bt_Parole_Passe.Background = Vert;
                NbParole++;
            }
        }

        


        private void Bt_Flop_1_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte(TB_F1, Bt_Main_2, Bt_Flop_1, 2);
        }

        private void Bt_Flop_2_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte(TB_F2, Bt_Flop_1, Bt_Flop_2, 3);
        }

        private void Bt_Flop_3_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte(TB_F3, Bt_Flop_2, Bt_Flop_3, 4);

            // 🔥 Analyse complète du flop
            Calcul_Flop();
        }
        /************************************
        **                                 **
        **              T U R N            **
        **                                 **
        ************************************/

        private void Bt_Turn_Click(object sender, RoutedEventArgs e)
        {
            //AfficherCarte(TB_T, Bt_Flop_3, Bt_Turn, 5);
            AfficherCarte_Test(TB_T, Bt_Turn, 5);

            // 🔥 Analyse Turn
            //Calcul_Turn();
        }
        private bool Tirage_Quinte_Special(List<int> valeurs)
        {
            // =====================================
            // Suppression doublons
            // =====================================

            valeurs =
                valeurs
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            // =====================================
            // Pas assez de cartes
            // =====================================

            if (valeurs.Count < 4)
                return false;

            // =====================================
            // Analyse groupes de 4 cartes
            // =====================================

            for (int i = 0;
                 i <= valeurs.Count - 4;
                 i++)
            {
                int a = valeurs[i];
                int b = valeurs[i + 1];
                int c = valeurs[i + 2];
                int d = valeurs[i + 3];

                // =====================================
                // QUINTE OUVERTE
                // Exemple :
                // 7 8 9 10
                // =====================================

                if (b == a + 1 &&
                    c == a + 2 &&
                    d == a + 3)
                {
                    return true;
                }

                // =====================================
                // VENTRALE 1
                // 7 8 10 J
                // manque 9
                // =====================================

                if (b == a + 1 &&
                    c == a + 3 &&
                    d == a + 4)
                {
                    return true;
                }

                // =====================================
                // VENTRALE 2
                // 7 9 10 J
                // manque 8
                // =====================================

                if (b == a + 2 &&
                    c == a + 3 &&
                    d == a + 4)
                {
                    return true;
                }

                // =====================================
                // VENTRALE 3
                // 7 8 9 J
                // manque 10
                // =====================================

                if (b == a + 1 &&
                    c == a + 2 &&
                    d == a + 4)
                {
                    return true;
                }
            }

            return false;
        }
        /************************************
        **                                 **
        **             R I V E R           **
        **                                 **
        ************************************/

        private void Bt_River_Click(object sender, RoutedEventArgs e)
        {
            //AfficherCarte(TB_R, Bt_Turn, Bt_River, 6);

            AfficherCarte_Test(TB_R, Bt_River, 6);
            // 🔥 Analyse River
            //Calcul_River();
        }
        private Brush CouleurCarte(string c)
        {
            if (c.Contains("♣")) return Brushes.Blue;
            if (c.Contains("♦")) return Brushes.Green;
            if (c.Contains("♥")) return Brushes.Red;
            if (c.Contains("♠")) return Brushes.Black;
            return Brushes.Black;
        }
        // =========================================================
        // FERMER
        // =========================================================
        private void Fermer_Click(object sender, RoutedEventArgs e)
        {
            StopTousModes();

            Close();
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
            Calcul_Flop();
            Calcul_Tirages_Flop();

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
            Calcul_Turn();
            Calcul_Tirages_Turn();

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
            Calcul_River();
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
            Bt_Flop_QFlush.Background = Blanc;
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
            Bt_Turn_QFlush.Background = Blanc;
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
            Bt_River_QFlush.Background = Blanc;
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
            Calcul_PreFlop();
            Calcul_Flop();
            Calcul_Tirages_Flop();
            Calcul_Turn();
            Calcul_Tirages_Turn();
            Calcul_River();
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
        private void Bt_Se_Recare_Click(object sender, RoutedEventArgs e)
        {
            if (Bt_Se_Recare.Background.Equals((Brush)FindResource("blanc")))
            {
                // 🔵 activation
                Bt_Se_Recare.Background = (Brush)FindResource("bleu");
            }
            else
            {
                // ⚪ désactivation
                Bt_Se_Recare.Background = (Brush)FindResource("blanc");

                // 🔥 FORCER AUTO OFF
                autoActif = false;
                Bt_Auto.Background = (Brush)FindResource("blanc");
            }
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
            if (EstDansRange(main))
            {
                RangeTapis.Remove(main);
                bouton.Background = (Brush)FindResource("blanc");
            }
            else
            {
                RangeTapis.Add(main);
                bouton.Background = (Brush)FindResource("gold");
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
            if (autoActif && Bt_Se_Recare.Background.Equals((Brush)FindResource("bleu")))
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
        private void Bt_Flop_QFlush_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Turn_Paire_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Turn_2_Paires_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Turn_Brelan_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Turn_Quinte_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Turn_Couleur_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Turn_Full_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Turn_Carre_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Turn_QFlush_Click(object sender, RoutedEventArgs e) { }
        private void Bt_River_Paire_Click(object sender, RoutedEventArgs e) { }
        private void Bt_River_2_Paires_Click(object sender, RoutedEventArgs e) { }
        private void Bt_River_Brelan_Click(object sender, RoutedEventArgs e) { }
        private void Bt_River_Quinte_Click(object sender, RoutedEventArgs e) { }
        private void Bt_River_Couleur_Click(object sender, RoutedEventArgs e) { }
        private void Bt_River_Full_Click(object sender, RoutedEventArgs e) { }
        private void Bt_River_Carre_Click(object sender, RoutedEventArgs e) { }
        private void Bt_River_QFlush_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Tir_Flop_Quinte_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Tir_Flop_Couleur_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Tir_Flop_QFlush_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Tir_Turn_Quinte_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Tir_Turn_Couleur_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Tir_Turn_QFlush_Click(object sender, RoutedEventArgs e) { }
        private void Bt_Suivre_Click(object sender, RoutedEventArgs e) { }
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
        private void Bt_Doublon_Click(object sender, RoutedEventArgs e)
        {

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
        private void ActiverPosition(Button boutonActif)
        {
            foreach (var b in BoutonsPosition)
                b.Background = (Brush)FindResource("blanc");

            boutonActif.Background = (Brush)FindResource("bleu");
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
        private void ActiverNiveauTable(Button boutonActif)
        {
            foreach (var b in BoutonsNiveauTable)
                b.Background = (Brush)FindResource("blanc");

            boutonActif.Background = (Brush)FindResource("bleu");
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
        private void Bt_NiveauTable_Click(object sender, RoutedEventArgs e)
        {
            ActiverNiveauTable((Button)sender);
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
        private void Bt_Benchmark_Click(object sender, RoutedEventArgs e)
        {

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
        private void AppliquerActionForcee()
        {
            // reset
            Bt_Tapis.Background = Blanc;
            Bt_Parole_Passe.Background = Blanc;
            Bt_Call.Background = Blanc;
            Bt_Raise.Background = Blanc;

            switch (ActionForcee)
            {
                case "TAPIS":
                    Bt_Tapis.Background = Vert;
                    break;

                case "PAROLE":
                    Bt_Parole_Passe.Background = Vert;
                    break;

                case "CALL":
                    Bt_Call.Background = Vert;
                    break;

                case "RAISE":
                    Bt_Raise.Background = Vert;
                    break;
            }
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
        private void Bt_Call_Click(object sender, RoutedEventArgs e)
        {
            ActionForcee = "CALL";
            AppliquerActionForcee();
        }
        private void Bt_Raise_Click(object sender, RoutedEventArgs e)
        {
            ActionForcee = "RAISE";
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
            if (Bt_Se_Recare.Background.Equals((Brush)FindResource("blanc")))
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
        private string NormaliserMain(string carte1, string carte2)
        {
            if (string.IsNullOrWhiteSpace(carte1) || string.IsNullOrWhiteSpace(carte2))
                return null;

            // sécurité longueur
            if (carte1.Length < 2 || carte2.Length < 2)
                return null;

            string v1 = carte1.Substring(0, carte1.Length - 1);
            string c1 = carte1.Substring(carte1.Length - 1);

            string v2 = carte2.Substring(0, carte2.Length - 1);
            string c2 = carte2.Substring(carte2.Length - 1);

            string ordre = "AKQJ1098765432";

            // sécurité valeur inconnue
            if (!ordre.Contains(v1) || !ordre.Contains(v2))
                return null;

            // mettre la plus forte en premier
            if (ordre.IndexOf(v2) < ordre.IndexOf(v1))
            {
                var tmpV = v1; v1 = v2; v2 = tmpV;
                var tmpC = c1; c1 = c2; c2 = tmpC;
            }

            // paire
            if (v1 == v2)
                return v1 + v2;

            // suited ?
            bool suited = (c1 == c2);

            return suited ? v1 + v2 + "p" : v1 + v2;
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

                // 🔥 gérer "10"
                if (i + 1 < brute.Length && brute[i] == '1' && brute[i + 1] == '0')
                {
                    valeur = "10";
                    i += 2;
                }
                else
                {
                    valeur = brute[i].ToString();
                    i += 1;
                }

                // 🔥 sécurité couleur
                if (i >= brute.Length)
                    return null;

                string couleur = brute[i].ToString();
                i += 1;

                cartes.Add(valeur + couleur);
            }

            if (cartes.Count < 2)
                return null;

            return NormaliserMain(cartes[0], cartes[1]);
        }
        private bool Detecter_Presence(string nom)
        {
            for (int i = 0; i < Main_BoardDetect.Length; i++)
            {
                // =====================================
                // Nom différent
                // =====================================

                if (Main_BoardDetect[i].Nom != nom)
                    continue;

                // =====================================
                // Lecture pixel
                // =====================================

                int couleur = LirePixel(Main_BoardDetect[i].X, Main_BoardDetect[i].Y);

                // =====================================
                // Couleur trouvée
                // =====================================

                if (couleur ==
                    Main_BoardDetect[i].Couleur)
                {
                    return true;
                }
            }

            return false;
        }
        private bool EstDansRange(string main)
        {
            // match exact
            if (RangeTapis.Contains(main))
                return true;

            // si main suited (ex: Q10p)
            if (main.EndsWith("p"))
            {
                string offsuit = main.Substring(0, main.Length - 1);

                if (RangeTapis.Contains(offsuit))
                    return true;
            }

            return false;
        }

        // =========================================================
        // 🔥 INITIALISE
        // =========================================================
        private void Bt_Initialise_Click(object sender, RoutedEventArgs e)
        {
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

            // 🔥 Reprise du timer (au cas où)
            if (timer != null)
                timer.Start();

            Console.WriteLine("SESSION RÉINITIALISÉE");
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
        
        // =====================================
        // EFFACER TIRAGES FLOP
        // =====================================
        private void Effacer_Tirages_Flop()
        {
            Bt_Tir_Flop_Quinte.IsEnabled = false;
            Bt_Tir_Flop_Couleur.IsEnabled = false;
            Bt_Tir_Flop_QFlush.IsEnabled = false;
        }

        // =====================================
        // EFFACER TIRAGES TURN
        // =====================================
        private void Effacer_Tirages_Turn()
        {
            Bt_Tir_Turn_Quinte.IsEnabled = false;
            Bt_Tir_Turn_Couleur.IsEnabled = false;
            Bt_Tir_Turn_QFlush.IsEnabled = false;
        }
        // =====================================
        // ALLUMER FLOP QUINTE
        // =====================================
        private void Flop_Quinte_ON()
        {
            Bt_Tir_Flop_Quinte.IsEnabled = true;
        }
        // =====================================
        // ALLUMER FLOP COULEUR
        // =====================================
        private void Flop_Couleur_ON()
        {
            Bt_Tir_Flop_Couleur.IsEnabled = true;
        }
        // =====================================
        // ALLUMER FLOP QFLUSH
        // =====================================
        private void Flop_QFlush_ON()
        {
            Bt_Tir_Flop_QFlush.IsEnabled = true;
        }
        // =====================================
        // ALLUMER TURN QUINTE
        // =====================================
        private void Turn_Quinte_ON()
        {
            Bt_Tir_Turn_Quinte.IsEnabled = true;
        }
        // =====================================
        // ALLUMER TURN COULEUR
        // =====================================
        private void Turn_Couleur_ON()
        {
            Bt_Tir_Turn_Couleur.IsEnabled = true;
        }
        // =====================================
        // ALLUMER TURN QFLUSH
        // =====================================
        private void Turn_QFlush_ON()
        {
            Bt_Tir_Turn_QFlush.IsEnabled = true;
        }
        // =====================================
        // TEST MANUEL
        // =====================================
        private void Test_Tirages()
        {
            Effacer_Tirages_Flop();
            Effacer_Tirages_Turn();

            Flop_Quinte_ON();
            Flop_Couleur_ON();

            Turn_QFlush_ON();
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

        // =========================================================
        // EFFACE BOARD
        // =========================================================
        private void Efface_Board()
        {
            TB_F1.Clear();
            TB_F2.Clear();
            TB_F3.Clear();
            TB_T.Clear();
            TB_R.Clear();
        }

        #region CALCUL PREFLOP

        // =========================================================
        // 🔥 CALCUL PREFLOP
        // =========================================================
        private void Calcul_PreFlop()
        {
            bPreFlop_Paire = false;
            bPreFlop_Pareille = false;
            bPreFlop_Suite = false;

            Bt_PreFlop_Paire.Background = Blanc;
            Bt_PreFlop_Pareille.Background = Blanc;
            Bt_PreFlop_Suite.Background = Blanc;

            string c1 = TB_M1.Text;
            string c2 = TB_M2.Text;

            if (string.IsNullOrWhiteSpace(c1) ||
                string.IsNullOrWhiteSpace(c2))
                return;

            // 🔥 Gestion correcte du 10
            string val1 = c1.StartsWith("10") ? "T" : c1[0].ToString();
            string val2 = c2.StartsWith("10") ? "T" : c2[0].ToString();

            char s1 = c1[c1.Length - 1];
            char s2 = c2[c2.Length - 1];

            string ordre = "23456789TJQKA";

            int i1 = ordre.IndexOf(val1);
            int i2 = ordre.IndexOf(val2);

            // 🔥 PAIRE
            if (val1 == val2)
            {
                bPreFlop_Paire = true;
                Bt_PreFlop_Paire.Background = Vert;
            }

            // 🔥 PAREILLE
            if (s1 == s2)
            {
                bPreFlop_Pareille = true;
                Bt_PreFlop_Pareille.Background = Vert;
            }

            // 🔥 SUITE
            if (Math.Abs(i1 - i2) == 1 ||
                (val1 == "A" && val2 == "2") ||
                (val1 == "2" && val2 == "A"))
            {
                bPreFlop_Suite = true;
                Bt_PreFlop_Suite.Background = Vert;
            }
        }

        #endregion

        #region CALCUL FLOP

        // =========================================================
        // 🔥 CALCUL PREFLOP
        // =========================================================
        private void Calcul_Flop()
        {
            // =========================================================
            // 🔥 RESET BOOLEANS
            // =========================================================

            bFlop_Paire = false;
            bFlop_2_Paires = false;
            bFlop_Brelan = false;
            bFlop_Quinte = false;
            bFlop_Couleur = false;
            bFlop_Full = false;
            bFlop_Carre = false;
            bFlop_QFlush = false;

            // =========================================================
            // 🔥 RESET BOUTONS
            // =========================================================

            Bt_Flop_Paire.Background = Blanc;
            Bt_Flop_2_Paires.Background = Blanc;
            Bt_Flop_Brelan.Background = Blanc;
            Bt_Flop_Quinte.Background = Blanc;
            Bt_Flop_Couleur.Background = Blanc;
            Bt_Flop_Full.Background = Blanc;
            Bt_Flop_Carre.Background = Blanc;
            Bt_Flop_QFlush.Background = Blanc;

            // =========================================================
            // 🔥 LECTURE CARTES
            // =========================================================

            List<string> cartes = new List<string>()
            {
                TB_M1.Text,
                TB_M2.Text,
                TB_F1.Text,
                TB_F2.Text,
                TB_F3.Text
            };

            // 🔒 Sécurité
            if (cartes.Any(c => string.IsNullOrWhiteSpace(c)))
                return;

            // =========================================================
            // 🔥 EXTRACTION VALEURS / COULEURS
            // =========================================================

            List<int> valeurs = new List<int>();
            List<char> couleurs = new List<char>();

            string ordre = "23456789TJQKA";

            foreach (string c in cartes)
            {
                string val = c.StartsWith("10")
                    ? "T"
                    : c[0].ToString();

                char suit = c[c.Length - 1];

                valeurs.Add(ordre.IndexOf(val) + 2);
                couleurs.Add(suit);
            }

            // =========================================================
            // 🔥 COMPTAGE VALEURS
            // =========================================================

            var groupesValeurs = valeurs
                .GroupBy(v => v)
                .Select(g => g.Count())
                .OrderByDescending(x => x)
                .ToList();

            // =========================================================
            // 🔥 PAIRE
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 2, 1, 1, 1 }))
            {
                bFlop_Paire = true;
                Bt_Flop_Paire.Background = Vert;
            }

            // =========================================================
            // 🔥 2 PAIRES
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 2, 2, 1 }))
            {
                bFlop_2_Paires = true;
                Bt_Flop_2_Paires.Background = Vert;
            }

            // =========================================================
            // 🔥 BRELAN
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 3, 1, 1 }))
            {
                bFlop_Brelan = true;
                Bt_Flop_Brelan.Background = Vert;
            }

            // =========================================================
            // 🔥 FULL
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 3, 2 }))
            {
                bFlop_Full = true;
                Bt_Flop_Full.Background = Vert;
            }

            // =========================================================
            // 🔥 CARRÉ
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 4, 1 }))
            {
                bFlop_Carre = true;
                Bt_Flop_Carre.Background = Vert;
            }

            // =========================================================
            // 🔥 COULEUR
            // =========================================================

            bool couleur =
                couleurs.GroupBy(c => c)
                        .Any(g => g.Count() >= 5);

            if (couleur)
            {
                bFlop_Couleur = true;
                Bt_Flop_Couleur.Background = Vert;
            }

            // =========================================================
            // 🔥 QUINTE
            // =========================================================

            var vals = valeurs
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            // 🔥 Gestion A bas
            if (vals.Contains(14))
                vals.Insert(0, 1);

            int suite = 1;

            for (int i = 1; i < vals.Count; i++)
            {
                if (vals[i] == vals[i - 1] + 1)
                {
                    suite++;

                    if (suite >= 5)
                    {
                        bFlop_Quinte = true;
                        Bt_Flop_Quinte.Background = Vert;
                    }
                }
                else
                {
                    suite = 1;
                }
            }

            // =========================================================
            // 🔥 QUINTE FLUSH
            // =========================================================

            if (bFlop_Quinte && bFlop_Couleur)
            {
                bFlop_QFlush = true;
                Bt_Flop_QFlush.Background = Vert;
            }
        }
        private void Calcul_Tirages_Flop()
        {
            // =========================================================
            // 🔥 RESET
            // =========================================================

            bTir_Flop_Quinte = false;
            bTir_Flop_Couleur = false;
            bTir_Flop_QFlush = false;

            Bt_Tir_Flop_Quinte.Background = Blanc;
            Bt_Tir_Flop_Couleur.Background = Blanc;
            Bt_Tir_Flop_QFlush.Background = Blanc;

            // =========================================================
            // 🔥 LECTURE CARTES
            // =========================================================

            List<string> cartes = new List<string>()
            {
                TB_M1.Text,
                TB_M2.Text,
                TB_F1.Text,
                TB_F2.Text,
                TB_F3.Text
            };

            if (cartes.Any(c => string.IsNullOrWhiteSpace(c)))
                return;

            List<int> valeurs = new List<int>();
            List<char> couleurs = new List<char>();

            string ordre = "23456789TJQKA";

            foreach (string c in cartes)
            {
                string val = c.StartsWith("10")
                    ? "T"
                    : c[0].ToString();

                char suit = c[c.Length - 1];

                valeurs.Add(ordre.IndexOf(val) + 2);
                couleurs.Add(suit);
            }

            // =========================================================
            // 🔥 TIRAGE COULEUR
            // 4 cartes même couleur
            // =========================================================

            bool tirCouleur =
                couleurs.GroupBy(c => c)
                        .Any(g => g.Count() == 4);

            if (tirCouleur)
            {
                bTir_Flop_Couleur = true;
                Bt_Tir_Flop_Couleur.Background = Vert;
            }

            // =========================================================
            // 🔥 TIRAGE QUINTE
            // 4 cartes qui se suivent
            // =========================================================

            var vals = valeurs
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            if (vals.Contains(14))
                vals.Insert(0, 1);

            int suite = 1;

            for (int i = 1; i < vals.Count; i++)
            {
                if (vals[i] == vals[i - 1] + 1)
                {
                    suite++;

                    if (suite == 4)
                    {
                        bTir_Flop_Quinte = true;
                        Bt_Tir_Flop_Quinte.Background = Vert;
                    }
                }
                else
                {
                    suite = 1;
                }
            }

            // =========================================================
            // 🔥 TIRAGE QUINTE FLUSH
            // =========================================================

            if (bTir_Flop_Quinte && bTir_Flop_Couleur)
            {
                bTir_Flop_QFlush = true;
                Bt_Tir_Flop_QFlush.Background = Vert;
            }
        }

        #endregion

        #region CALCUL TURN

        // =========================================================
        // 🔥 CALCUL TURN
        // =========================================================
        private void Calcul_Turn()
        {
            // =========================================================
            // 🔥 RESET BOOLEANS
            // =========================================================

            bTurn_Paire = false;
            bTurn_2_Paires = false;
            bTurn_Brelan = false;
            bTurn_Quinte = false;
            bTurn_Couleur = false;
            bTurn_Full = false;
            bTurn_Carre = false;
            bTurn_QFlush = false;

            // =========================================================
            // 🔥 RESET BOUTONS
            // =========================================================

            Bt_Turn_Paire.Background = Blanc;
            Bt_Turn_2_Paires.Background = Blanc;
            Bt_Turn_Brelan.Background = Blanc;
            Bt_Turn_Quinte.Background = Blanc;
            Bt_Turn_Couleur.Background = Blanc;
            Bt_Turn_Full.Background = Blanc;
            Bt_Turn_Carre.Background = Blanc;
            Bt_Turn_QFlush.Background = Blanc;

            // =========================================================
            // 🔥 LECTURE CARTES
            // =========================================================

            List<string> cartes = new List<string>()
            {
                TB_M1.Text,
                TB_M2.Text,

                TB_F1.Text,
                TB_F2.Text,
                TB_F3.Text,

                TB_T.Text
            };

            // 🔒 Sécurité
            if (cartes.Any(c => string.IsNullOrWhiteSpace(c)))
                return;

            // =========================================================
            // 🔥 EXTRACTION VALEURS / COULEURS
            // =========================================================

            List<int> valeurs = new List<int>();
            List<char> couleurs = new List<char>();

            string ordre = "23456789TJQKA";

            foreach (string c in cartes)
            {
                string val = c.StartsWith("10")
                    ? "T"
                    : c[0].ToString();

                char suit = c[c.Length - 1];

                valeurs.Add(ordre.IndexOf(val) + 2);
                couleurs.Add(suit);
            }

            // =========================================================
            // 🔥 COMPTAGE VALEURS
            // =========================================================

            var groupesValeurs = valeurs
                .GroupBy(v => v)
                .Select(g => g.Count())
                .OrderByDescending(x => x)
                .ToList();

            // =========================================================
            // 🔥 PAIRE
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 2, 1, 1, 1, 1 }))
            {
                bTurn_Paire = true;
                Bt_Turn_Paire.Background = Vert;
            }

            // =========================================================
            // 🔥 2 PAIRES
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 2, 2, 1, 1 }))
            {
                bTurn_2_Paires = true;
                Bt_Turn_2_Paires.Background = Vert;
            }

            // =========================================================
            // 🔥 BRELAN
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 3, 1, 1, 1 }))
            {
                bTurn_Brelan = true;
                Bt_Turn_Brelan.Background = Vert;
            }

            // =========================================================
            // 🔥 FULL
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 3, 2, 1 }))
            {
                bTurn_Full = true;
                Bt_Turn_Full.Background = Vert;
            }

            // =========================================================
            // 🔥 CARRÉ
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 4, 1, 1 }))
            {
                bTurn_Carre = true;
                Bt_Turn_Carre.Background = Vert;
            }

            // =========================================================
            // 🔥 COULEUR
            // =========================================================

            bool couleur =
                couleurs.GroupBy(c => c)
                        .Any(g => g.Count() >= 5);

            if (couleur)
            {
                bTurn_Couleur = true;
                Bt_Turn_Couleur.Background = Vert;
            }

            // =========================================================
            // 🔥 QUINTE
            // =========================================================

            var vals = valeurs
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            // 🔥 Gestion As bas
            if (vals.Contains(14))
                vals.Insert(0, 1);

            int suite = 1;

            for (int i = 1; i < vals.Count; i++)
            {
                if (vals[i] == vals[i - 1] + 1)
                {
                    suite++;

                    if (suite >= 5)
                    {
                        bTurn_Quinte = true;
                        Bt_Turn_Quinte.Background = Vert;
                    }
                }
                else
                {
                    suite = 1;
                }
            }

            // =========================================================
            // 🔥 QUINTE FLUSH
            // =========================================================

            if (bTurn_Quinte && bTurn_Couleur)
            {
                bTurn_QFlush = true;
                Bt_Turn_QFlush.Background = Vert;
            }
        }
        private void Calcul_Tirages_Turn()
        {
            // =========================================================
            // 🔥 RESET
            // =========================================================

            bTir_Turn_Quinte = false;
            bTir_Turn_Couleur = false;
            bTir_Turn_QFlush = false;

            Bt_Tir_Turn_Quinte.Background = Blanc;
            Bt_Tir_Turn_Couleur.Background = Blanc;
            Bt_Tir_Turn_QFlush.Background = Blanc;

            // =========================================================
            // 🔥 LECTURE CARTES
            // =========================================================

            List<string> cartes = new List<string>()
            {
                TB_M1.Text,
                TB_M2.Text,

                TB_F1.Text,
                TB_F2.Text,
                TB_F3.Text,

                TB_T.Text
            };

            // =========================================================
            // 🔥 SECURITE
            // =========================================================

            if (cartes.Any(c =>
                string.IsNullOrWhiteSpace(c)))
            {
                return;
            }

            // =========================================================
            // 🔥 EXTRACTION VALEURS / COULEURS
            // =========================================================

            List<int> valeurs =
                new List<int>();

            List<char> couleurs =
                new List<char>();

            // =========================================================
            // 🔥 LISTES PAR COULEUR
            // =========================================================

            List<int> Trefles =
                new List<int>();

            List<int> Piques =
                new List<int>();

            List<int> Coeurs =
                new List<int>();

            List<int> Carreaux =
                new List<int>();

            string ordre =
                "23456789TJQKA";

            foreach (string c in cartes)
            {
                // =====================================
                // Valeur
                // =====================================

                string val =
                    c.StartsWith("10")
                    ? "T"
                    : c[0].ToString();

                int v =
                    ordre.IndexOf(val) + 2;

                // =====================================
                // Couleur
                // =====================================

                char suit =
                    c[c.Length - 1];

                valeurs.Add(v);
                couleurs.Add(suit);

                // =====================================
                // Répartition couleur
                // =====================================

                if (suit == '♣')
                    Trefles.Add(v);

                if (suit == '♠')
                    Piques.Add(v);

                if (suit == '♥')
                    Coeurs.Add(v);

                if (suit == '♦')
                    Carreaux.Add(v);
            }

            // =========================================================
            // 🔥 TIRAGE COULEUR
            // 4 cartes même couleur
            // =========================================================

            bool tirCouleur =
                couleurs
                .GroupBy(c => c)
                .Any(g => g.Count() >= 4);

            if (tirCouleur)
            {
                bTir_Turn_Couleur = true;

                Bt_Tir_Turn_Couleur.Background =
                    Vert;
            }

            // =========================================================
            // 🔥 TIRAGE QUINTE
            // UNIQUEMENT SUITE OUVERTE
            // =========================================================

            var vals =
                valeurs
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            // =====================================
            // Gestion As bas
            // =====================================

            if (vals.Contains(14))
            {
                vals.Insert(0, 1);
            }

            int suite = 1;

            for (int i = 1;
                 i < vals.Count;
                 i++)
            {
                if (vals[i] ==
                    vals[i - 1] + 1)
                {
                    suite++;

                    if (suite >= 4)
                    {
                        bTir_Turn_Quinte = true;

                        Bt_Tir_Turn_Quinte.Background =
                            Vert;
                    }
                }
                else
                {
                    suite = 1;
                }
            }

            // =========================================================
            // 🔥 TIRAGE QUINTE FLUSH
            // =========================================================

            if (Tirage_Quinte_Special(Trefles) ||
                Tirage_Quinte_Special(Piques) ||
                Tirage_Quinte_Special(Coeurs) ||
                Tirage_Quinte_Special(Carreaux))
            {
                bTir_Turn_QFlush = true;

                Bt_Tir_Turn_QFlush.Background =
                    Vert;
            }
            else
            {
                bTir_Turn_QFlush = false;

                Bt_Tir_Turn_QFlush.Background =
                    Blanc;
            }
        }

        #endregion

        #region CALCUL RIVER

        // =========================================================
        // 🔥 CALCUL RIVER
        // =========================================================
        private void Calcul_River()
        {
            // =========================================================
            // 🔥 RESET BOOLEANS
            // =========================================================

            bRiver_Paire = false;
            bRiver_2_Paires = false;
            bRiver_Brelan = false;
            bRiver_Quinte = false;
            bRiver_Couleur = false;
            bRiver_Full = false;
            bRiver_Carre = false;
            bRiver_QFlush = false;

            // =========================================================
            // 🔥 RESET BOUTONS
            // =========================================================

            Bt_River_Paire.Background = Blanc;
            Bt_River_2_Paires.Background = Blanc;
            Bt_River_Brelan.Background = Blanc;
            Bt_River_Quinte.Background = Blanc;
            Bt_River_Couleur.Background = Blanc;
            Bt_River_Full.Background = Blanc;
            Bt_River_Carre.Background = Blanc;
            Bt_River_QFlush.Background = Blanc;

            // =========================================================
            // 🔥 LECTURE DES 7 CARTES
            // =========================================================

            List<string> cartes = new List<string>()
            {
                TB_M1.Text,
                TB_M2.Text,

                TB_F1.Text,
                TB_F2.Text,
                TB_F3.Text,

                TB_T.Text,
                TB_R.Text
            };

            // 🔒 Sécurité
            if (cartes.Any(c => string.IsNullOrWhiteSpace(c)))
                return;

            // =========================================================
            // 🔥 EXTRACTION VALEURS / COULEURS
            // =========================================================

            List<int> valeurs = new List<int>();
            List<char> couleurs = new List<char>();

            string ordre = "23456789TJQKA";

            foreach (string c in cartes)
            {
                string val = c.StartsWith("10")
                    ? "T"
                    : c[0].ToString();

                char suit = c[c.Length - 1];

                valeurs.Add(ordre.IndexOf(val) + 2);
                couleurs.Add(suit);
            }

            // =========================================================
            // 🔥 COMPTAGE VALEURS
            // =========================================================

            var groupesValeurs = valeurs
                .GroupBy(v => v)
                .Select(g => g.Count())
                .OrderByDescending(x => x)
                .ToList();

            // =========================================================
            // 🔥 PAIRE
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 2, 1, 1, 1, 1, 1 }))
            {
                bRiver_Paire = true;
                Bt_River_Paire.Background = Vert;
            }

            // =========================================================
            // 🔥 2 PAIRES
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 2, 2, 1, 1, 1 }))
            {
                bRiver_2_Paires = true;
                Bt_River_2_Paires.Background = Vert;
            }

            // =========================================================
            // 🔥 BRELAN
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 3, 1, 1, 1, 1 }))
            {
                bRiver_Brelan = true;
                Bt_River_Brelan.Background = Vert;
            }

            // =========================================================
            // 🔥 FULL
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 3, 2, 1, 1 }))
            {
                bRiver_Full = true;
                Bt_River_Full.Background = Vert;
            }

            // =========================================================
            // 🔥 CARRÉ
            // =========================================================

            if (groupesValeurs.SequenceEqual(new List<int> { 4, 1, 1, 1 }))
            {
                bRiver_Carre = true;
                Bt_River_Carre.Background = Vert;
            }

            // =========================================================
            // 🔥 COULEUR
            // =========================================================

            bool couleur =
                couleurs.GroupBy(c => c)
                        .Any(g => g.Count() >= 5);

            if (couleur)
            {
                bRiver_Couleur = true;
                Bt_River_Couleur.Background = Vert;
            }

            // =========================================================
            // 🔥 QUINTE
            // =========================================================

            var vals = valeurs
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            // 🔥 Gestion As bas
            if (vals.Contains(14))
                vals.Insert(0, 1);

            int suite = 1;

            for (int i = 1; i < vals.Count; i++)
            {
                if (vals[i] == vals[i - 1] + 1)
                {
                    suite++;

                    if (suite >= 5)
                    {
                        bRiver_Quinte = true;
                        Bt_River_Quinte.Background = Vert;
                    }
                }
                else
                {
                    suite = 1;
                }
            }

            // =========================================================
            // 🔥 QUINTE FLUSH
            // =========================================================

            if (bRiver_Quinte && bRiver_Couleur)
            {
                bRiver_QFlush = true;
                Bt_River_QFlush.Background = Vert;
            }
        }

        #endregion     

        #region CHARGER SAUVER LIRE EXTRAIRE
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
        private void Sauver_Main1_JSON()
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
        private void Sauver_Main2_JSON()
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
        private void Sauver_Flop1_JSON()
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
        private void Sauver_Flop2_JSON()
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
        private void Sauver_Flop3_JSON()
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
        private void Sauver_Turn_JSON()
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
        private void Sauver_Turn_HB_JSON()
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
        private void Sauver_River_JSON()
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
        private void Sauver_River_HB_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            String fichier = Path.Combine(DOSSIER_JSON, "River_HB.json");
            string json = JsonConvert.SerializeObject(River_HBDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
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

            FelicitationDetect = JsonConvert.DeserializeObject<S_FelicitationDetect[]>(json);
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

                    FelicitationDetect[index].Nom = nomCourant;
                    FelicitationDetect[index].X = int.Parse(t[1]);
                    FelicitationDetect[index].Y = int.Parse(t[2]);
                    FelicitationDetect[index].Couleur = int.Parse(t[3]);

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
        private void Sauver_Felicitation_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Felicitation.json");
            string json = JsonConvert.SerializeObject(PlusDeJetonsDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
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
        private void Sauver_Joueur_A_Tapis_JSON()
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
        private void Sauver_Joueur_Parole_JSON()
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
        private void Sauver_Joueur_Reflechit_JSON()
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
        private void Sauver_Joueur_Relance_JSON()
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
        private void Sauver_Joueur_Se_Couche_JSON()
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
        private void Sauver_Joueur_Suit_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Joueur_Suit.json");
            string json = JsonConvert.SerializeObject(JoueurSuitDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
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

            Main_BoardDetect = JsonConvert.DeserializeObject<S_CarteDetect[]>(json);
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

                    MessageBox.Show("bMain_Board = true;");
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

                    Main_BoardDetect[index].Nom = nomCourant;

                    Main_BoardDetect[index].X = int.Parse(t[1]);

                    Main_BoardDetect[index].Y = int.Parse(t[2]);

                    Main_BoardDetect[index].Couleur = int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // DONNEES NON CONFORMES
            // =====================================
            if (index != 83)
            {
                MessageBox.Show("index != 83");
                return false;
            }

            return true;
        }
        private void Sauver_Main_Board_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            String fichier = Path.Combine(DOSSIER_JSON, "Main_Board.json");
            string json = JsonConvert.SerializeObject(Main_BoardDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

        // =========================================================
        // 🔥 MA POSITION
        // =========================================================
        private void Charger_Ma_Position_JSON()
        {
            string fichier = Path.Combine(DOSSIER_JSON, "Ma_Position.json");

            if (!File.Exists(fichier))
                return;

            string json = File.ReadAllText(fichier);

            MaPositionDetect = JsonConvert.DeserializeObject<S_MaPositionDetect[]>(json);
        }
        private bool Extraire_Ma_Position(string txt)
        {
            string[] lignes = txt.Split('\n');
            int index = 0;
            bool bMa_Position = false;
            string nomCourant = "";
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                // =====================================
                // DEBUT DU BLOC
                // =====================================
                if (ligne == "COMMENT : Ma_Position")
                {
                    bMa_Position = true;
                    continue;
                }

                // =====================================
                // PAS ENCORE DANS LE BLOC
                // =====================================
                if (!bMa_Position)
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

                    MaPositionDetect[index].Nom = nomCourant;
                    MaPositionDetect[index].X = int.Parse(t[1]);
                    MaPositionDetect[index].Y = int.Parse(t[2]);
                    MaPositionDetect[index].Couleur = int.Parse(t[3]);

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
        private void Sauver_Ma_Position_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Ma_Position.json");
            string json = JsonConvert.SerializeObject(MaPositionDetect, Formatting.Indented);
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
        private void Sauver_Niveau_Table_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Niveau_Table.json");
            string json = JsonConvert.SerializeObject(NiveauTableDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
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
        private void Sauver_Nouveau_Niveau_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Nouveau_Niveau.json");
            string json = JsonConvert.SerializeObject(NouveauNiveauDetect, Formatting.Indented);
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
        private void Sauver_Pupitre_Gris_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Pupitre_Gris.json");
            string json = JsonConvert.SerializeObject(PupitreGrisDetect, Formatting.Indented);
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

            PlacesLibresDetect = JsonConvert.DeserializeObject<S_PlacesLibresDetect[]>(json);
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

                    PlacesLibresDetect[index].Nom = nomCourant;
                    PlacesLibresDetect[index].X = int.Parse(t[1]);
                    PlacesLibresDetect[index].Y = int.Parse(t[2]);
                    PlacesLibresDetect[index].Couleur = int.Parse(t[3]);

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
        private void Sauver_Places_Libres_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Places_Libres.json");
            string json = JsonConvert.SerializeObject(PlacesLibresDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
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
        private void Sauver_Plus_De_Jetons_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Plus_De_Jetons.json");
            string json = JsonConvert.SerializeObject(PlusDeJetonsDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
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

            PlusDePotDetect = JsonConvert.DeserializeObject<S_PlusDePotDetect[]>(json);
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

            if (index != 1)
                return false;

            return true;
        }
        private void Sauver_Plus_De_Pot_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Plus_De_Pot.json");
            string json = JsonConvert.SerializeObject(PlusDePotDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }

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
        private void Sauver_Position_Bouton_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Position_Bouton.json");
            string json = JsonConvert.SerializeObject(PositionBoutonDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
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
        private void Sauver_Publicite_JSON()
        {
            Directory.CreateDirectory(DOSSIER_JSON);
            string fichier = Path.Combine(DOSSIER_JSON, "Publicite.json");
            string json = JsonConvert.SerializeObject(PubliciteDetect, Formatting.Indented);
            File.WriteAllText(fichier, json);
        }


        #endregion

        #region DETECTIONS

        // =========================================================
        // DETECTER MA POSITION
        // =========================================================
        private int Ma_Position_Detectee()
        {
            for (int i = 0; i < MaPositionDetect.Length; i++)
            {
                int couleur = LirePixel(
                    MaPositionDetect[i].X,
                    MaPositionDetect[i].Y);

                if (couleur == MaPositionDetect[i].Couleur)
                    return i + 1;   // Position 1 à 9
            }

            return 0;               // Non détectée
        }
        // =========================================================
        // DETECTER PLUS DE JETONS
        // =========================================================
        private bool Plus_De_Jetons_Detecte()
        {
            for (int i = 0; i < PlusDeJetonsDetect.Length; i++)
            {
                int couleur = LirePixel(
                    PlusDeJetonsDetect[i].X,
                    PlusDeJetonsDetect[i].Y);

                if (couleur == PlusDeJetonsDetect[i].Couleur)
                    return true;
            }

            return false;
        }
        // =========================================================
        // DETECTER PLUS DE POT
        // =========================================================
        private bool Plus_De_Pot_Detecte()
        {
            for (int i = 0; i < PlusDePotDetect.Length; i++)
            {
                int couleur = LirePixel(
                    PlusDePotDetect[i].X,
                    PlusDePotDetect[i].Y);

                if (couleur == PlusDePotDetect[i].Couleur)
                    return true;
            }

            return false;
        }
        // =========================================================
        // DETECTER PUPITRE GRIS
        // =========================================================
        private bool Pupitre_Gris_Detecte()
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

        #endregion

        #region INJECTION

        // =========================================================
        // 🔥 BOUTON INJECTION
        // =========================================================
        private void Bt_Injection_Click(object sender, RoutedEventArgs e)
        {
            // =====================================
            // Lecture presse-papiers
            // =====================================

            string txt = Clipboard.GetText();

            // =====================================
            // Rien ?
            // =====================================

            if (string.IsNullOrWhiteSpace(txt))
            {
                MessageBox.Show("Aucune donnée exploitable");
                return;
            }

            // =====================================
            // Affichage brut injection
            // =====================================

            TB_Injection.Text = txt;

            // =====================================
            // EXTRACTION
            // =====================================

            bool okMain1 = Extraire_Main1(txt);
            bool okMain2 = Extraire_Main2(txt);
            bool okFlop1 = Extraire_Flop1(txt);
            bool okFlop2 = Extraire_Flop2(txt);
            bool okFlop3 = Extraire_Flop3(txt);
            bool okTurn = Extraire_Turn(txt);
            bool okTurn_HB = Extraire_Turn_HB(txt);
            bool okRiver = Extraire_River(txt);
            bool okRiver_HB = Extraire_River_HB(txt);
            bool okMain_Board = Extraire_Main_Board(txt);
            bool okMa_Position = Extraire_Ma_Position(txt);
            bool okPlaces_Libres = Extraire_Places_Libres(txt);
            bool okNiveau_Table = Extraire_Niveau_Table(txt);
            bool okPosition_Bouton = Extraire_Position_Bouton(txt);
            bool okPublicite = Extraire_Publicite(txt);
            bool okPlus_De_Jetons = Extraire_Plus_De_Jetons(txt);
            bool okFelicitation = Extraire_Felicitation(txt);
            bool okNouveau_Niveau = Extraire_Nouveau_Niveau(txt);
            bool okPlus_De_Pot = Extraire_Plus_De_Pot(txt);
            bool okPupitre_Gris = Extraire_Pupitre_Gris(txt);

            bool okJoueur_Parole = Extraire_Joueur_Parole(txt);
            bool okJoueur_A_Tapis = Extraire_Joueur_A_Tapis(txt);
            bool okJoueur_Reflechit = Extraire_Joueur_Reflechit(txt);
            bool okJoueur_Relance = Extraire_Joueur_Relance(txt);
            bool okJoueur_Se_Couche = Extraire_Joueur_Se_Couche(txt);
            bool okJoueur_Suit = Extraire_Joueur_Suit(txt);

            // =====================================
            // Aucune donnée reconnue
            // =====================================

            if (!okMain1 &&
                !okMain2 &&
                !okFlop1 &&
                !okFlop2 &&
                !okFlop3 &&
                !okTurn &&
                !okTurn_HB &&
                !okRiver &&
                !okRiver_HB &&
                !okMain_Board &&
                !okMa_Position &&
                !okPlaces_Libres &&
                !okNiveau_Table &&
                !okJoueur_Parole &&
                !okJoueur_A_Tapis &&
                !okJoueur_Reflechit &&
                !okJoueur_Relance &&
                !okJoueur_Se_Couche &&
                !okJoueur_Suit &&
                !okPosition_Bouton &&
                !okPublicite &&
                !okPlus_De_Jetons &&
                !okFelicitation &&
                !okNouveau_Niveau &&
                !okPlus_De_Pot &&
                !okPupitre_Gris)
            {
                MessageBox.Show("Injection refusée");
                return;
            }

            // =====================================
            // SAUVEGARDE JSON
            // =====================================

            if (okMain1)
                Sauver_Main1_JSON();

            if (okMain2)
                Sauver_Main2_JSON();

            if (okFlop1)
                Sauver_Flop1_JSON();

            if (okFlop2)
                Sauver_Flop2_JSON();

            if (okFlop3)
                Sauver_Flop3_JSON();

            if (okTurn)
                Sauver_Turn_JSON();

            if (okTurn_HB)
                Sauver_Turn_HB_JSON();

            if (okRiver)
                Sauver_River_JSON();

            if (okRiver_HB)
                Sauver_River_HB_JSON();

            if (okMain_Board)
                Sauver_Main_Board_JSON();

            if (okMa_Position)
                Sauver_Ma_Position_JSON();

            if (okPlaces_Libres)
                Sauver_Places_Libres_JSON();

            if (okNiveau_Table)
                Sauver_Niveau_Table_JSON();

            if (okPosition_Bouton)
                Sauver_Position_Bouton_JSON();

            if (okPublicite)
                Sauver_Publicite_JSON();

            if (okPlus_De_Jetons)
                Sauver_Plus_De_Jetons_JSON();

            if (okPlus_De_Pot)
                Sauver_Plus_De_Pot_JSON();

            if (okPupitre_Gris)
                Sauver_Pupitre_Gris_JSON();

            if (okFelicitation)
                Sauver_Felicitation_JSON();

            if (okNouveau_Niveau)
                Sauver_Nouveau_Niveau_JSON();



            if (okJoueur_Parole)
                Sauver_Joueur_Parole_JSON();

            if (okJoueur_A_Tapis)
                Sauver_Joueur_A_Tapis_JSON();

            if (okJoueur_Reflechit)
                Sauver_Joueur_Reflechit_JSON();

            if (okJoueur_Relance)
                Sauver_Joueur_Relance_JSON();

            if (okJoueur_Se_Couche)
                Sauver_Joueur_Se_Couche_JSON();

            if (okJoueur_Suit)
                Sauver_Joueur_Suit_JSON();

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

            if (okMain_Board)
                msg += "Main_Board : 83 détecteurs\n";




            if (okJoueur_A_Tapis)
                msg += "Joueur_A_Tapis : 9 détecteurs\n";

            if (okJoueur_Parole)
                msg += "Joueur_Parole : 9 détecteurs\n";

            if (okJoueur_Reflechit)
                msg += "Joueur_Reflechit : 9 détecteur\n";

            if (okJoueur_Relance)
                msg += "Joueur_Relance : 9 détecteurs\n";

            if (okJoueur_Se_Couche)
                msg += "Joueur_Se_Couche : 9 détecteurs\n";

            if (okJoueur_Suit)
                msg += "Joueur_Suit : 9 détecteurs\n";




            if (okMa_Position)
                msg += "Ma_Position : 9 détecteurs\n";

            if (okPlaces_Libres)
                msg += "Places_Libres : 9 détecteurs\n";

            if (okNiveau_Table)
                msg += "Niveau_Table : 12 détecteurs\n";

            if (okPosition_Bouton)
                msg += "Position_Bouton : 9 détecteurs\n";

            if (okPublicite)
                msg += "Publicite : 1 détecteur\n";

            if (okPlus_De_Jetons)
                msg += "Plus_De_Jetons : 1 détecteur\n";

            if (okPlus_De_Pot)
                msg += "Plus_De_Pot : 1 détecteur\n";

            if (okFelicitation)
                msg += "Felicitation : 1 détecteur\n";

            if (okNouveau_Niveau)
                msg += "Nouveau_Niveau : 1 détecteur\n";

            if (okPupitre_Gris)
                msg += "Pupitre_Gris : 2 détecteurs\n";


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
        private void Bt_Mem01_Click(object sender, RoutedEventArgs e)
        {
            ActiverMemo(Bt_Mem01);
            ChargerRangeTapis(new List<string>
            {
                "AA","KK"
            });
        }
        private void Bt_Mem02_Click(object sender, RoutedEventArgs e)
        {
            ActiverMemo(Bt_Mem02);
            ChargerRangeTapis(new List<string>
            {
                "AA",   "KK",   "QQ",
                "AKp"
            });
        }
        private void Bt_Mem03_Click(object sender, RoutedEventArgs e)
        {
            ActiverMemo(Bt_Mem03);
            ChargerRangeTapis(new List<string>
            {
                "AA",   "KK",   "QQ",
                "AKp",  "AQp",
                "AK"
            });
        }
        private void Bt_Mem04_Click(object sender, RoutedEventArgs e)
        {
            ActiverMemo(Bt_Mem04);
            ChargerRangeTapis(new List<string>
            {
                "AA",   "KK",   "QQ",   "JJ",   "1010", "99",   "88",
                "AKp",  "AQp",  "AJp",
                "AK",   "AQ"
            });
        }
        private void Bt_Mem05_Click(object sender, RoutedEventArgs e)
        {
            ActiverMemo(Bt_Mem05);
            ChargerRangeTapis(new List<string>
            {
                "AA",   "KK",   "QQ",   "JJ",   "1010", "99",   "88",   "77",   "66",
                "AKp",  "AQp",  "AJp",  "A10p",
                "AK",   "AQ",   "AJ",
                "KQp"
            });
        }
        private void Bt_Mem06_Click(object sender, RoutedEventArgs e)
        {
            ActiverMemo(Bt_Mem06);
            ChargerRangeTapis(new List<string>
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
            ActiverMemo(Bt_Mem07);
            ChargerRangeTapis(new List<string>
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
            ActiverMemo(Bt_Mem08);
            ChargerRangeTapis(new List<string>
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
            ActiverMemo(Bt_Mem09);
            ChargerRangeTapis(new List<string>
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
            ActiverMemo(Bt_Mem10);
            ChargerRangeTapis(new List<string>
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
            ActiverMemo(Bt_Mem11);
            ChargerRangeTapis(new List<string>
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
            ActiverMemo(Bt_Mem12);
            ChargerRangeTapis(new List<string>
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
            ActiverMemo(Bt_Mem13);
            ChargerRangeTapis(new List<string>
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
            // =====================================
            // BOARD
            // =====================================
            TB_M1.Clear();
            TB_M2.Clear();
            TB_F1.Clear();
            TB_F2.Clear();
            TB_F3.Clear();
            TB_T.Clear();
            TB_R.Clear();

            // =====================================
            // BOUTONS
            // =====================================
            Bt_Main_1.Background = Brushes.White;
            Bt_Main_2.Background = Brushes.White;
            Bt_Flop_1.Background = Brushes.White;
            Bt_Flop_2.Background = Brushes.White;
            Bt_Flop_3.Background = Brushes.White;
            Bt_Turn.Background = Brushes.White;
            Bt_River.Background = Brushes.White;

            // =====================================
            // CALCULS
            // =====================================
            Reset_PreFlop();
            Reset_Flop();
            Reset_Tirages_Flop();
            Reset_Turn();
            Reset_Tirages_Turn();
            Reset_River();
        }
        
        // =========================================================
        // RESET BOARD
        // =========================================================
        private void Reset_Board()
        {
            // =====================================
            // BOARD
            // =====================================
            TB_F1.Clear();
            TB_F2.Clear();
            TB_F3.Clear();
            TB_T.Clear();
            TB_R.Clear();

            //// =====================================
            //// BOUTONS
            //// =====================================
            //Bt_Flop_1.Background = Brushes.White;
            //Bt_Flop_2.Background = Brushes.White;
            //Bt_Flop_3.Background = Brushes.White;
            //Bt_Turn.Background = Brushes.White;
            //Bt_River.Background = Brushes.White;
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
            Bt_Flop_QFlush.Background = Blanc;
        }
        private void Reset_Tirages_Flop()
        {
            bTir_Flop_Quinte = false;
            bTir_Flop_Couleur = false;
            bTir_Flop_QFlush = false;

            Bt_Tir_Flop_Quinte.Background = Blanc;
            Bt_Tir_Flop_Couleur.Background = Blanc;
            Bt_Tir_Flop_QFlush.Background = Blanc;
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
            Bt_Turn_QFlush.Background = Blanc;
        }
        private void Reset_Tirages_Turn()
        {
            bTir_Turn_Quinte = false;
            bTir_Turn_Couleur = false;
            bTir_Turn_QFlush = false;

            Bt_Tir_Turn_Quinte.Background = Blanc;
            Bt_Tir_Turn_Couleur.Background = Blanc;
            Bt_Tir_Turn_QFlush.Background = Blanc;
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
            Bt_River_QFlush.Background = Blanc;
        }

        // =========================================================
        // RESET ETATS TRIGGER
        // =========================================================
        private void Reset_Etats_Trigger()
        {
            bMain1_OK = false;
            bMain2_OK = false;

            bFlop1_OK = false;
            bFlop2_OK = false;
            bFlop3_OK = false;

            bTurn_OK = false;

            bRiver_OK = false;
        }

        #endregion

        #region SE RECAVER
        // =========================================================
        // SE RECAVER
        // =========================================================
        private void Se_Recaver() // PRIVATE : Visible uniquement ici.
        {
            //MessageBox.Show("Se_Recaver");
            switch (MiseSelectionnee)
            {
                case -9: Recave_M(9); break;
                case -8: Recave_M(8); break;
                case -6: Recave_M(6); break;
                case -4: Recave_M(4); break;
                case -2: Recave_M(2); break;

                case 0:
                    // Aucun déplacement.
                    break;

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
        private void Recave_M(int Nombre) // PRIVATE : Visible uniquement ici.
        {
            for (int i = 0; i < Nombre; i++)
            {
                Clic(845, 560);
            }
        }

        // =========================================================
        // RECAVE PLUS
        // =========================================================
        private void Recave_P(int Nombre) // PRIVATE : Visible uniquement ici.
        {
            for (int i = 0; i < Nombre; i++)
            {
                Clic(1070, 560);
            }
        }

        // =========================================================
        // CLIC
        // =========================================================
        private void Clic(int X, int Y)
        {

            SetCursorPos(X, Y);

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }

        #endregion

        #region TEST_BOARD TRIGGERS_BOARD TAPIS_PREFLOP 

        // =========================================================
        // 🔥 TEST_BOARD
        // =========================================================
        private async void Bt_Test_Board_Click(object sender, RoutedEventArgs e)
        {
            // =====================================
            // STOP
            // =====================================
            if (bTestBoard)
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

            bTestBoard = true;
            // =====================================
            // BOUTONS
            // =====================================

            Bt_Main_1.Background = Brushes.White;
            Bt_Main_2.Background = Brushes.White;
            Bt_Flop_1.Background = Brushes.White;
            Bt_Flop_2.Background = Brushes.White;
            Bt_Flop_3.Background = Brushes.White;
            Bt_Turn.Background = Brushes.White;
            Bt_River.Background = Brushes.White;

            Bt_Test_Board.Background = Brushes.LightGreen;

            try
            {
                bTestBoard_Actif = true;

                // =====================================
                // BOUCLE
                // =====================================

                while (bTestBoard)
                {
                    // =====================================
                    // Capture écran
                    // =====================================
                    CaptureEcran();

                    // ==========================
                    // PASSAGE AU GRIS
                    // ==========================

                    if (Pupitre_Gris_Detecte())
                    {
                        bGris = true;
                    }

                    // ==========================
                    // RETOUR AU BLANC
                    // ==========================

                    if (bGris &&
                        !Pupitre_Gris_Detecte())
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
                    else if (Detecter_Presence("Turn_HB_Presence"))
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
                    else if (Detecter_Presence("River_HB_Presence"))
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

                bTestBoard = false;

                Bt_Test_Board.Background =
                    Brushes.White;
            }
        }

        // =========================================================
        // TRIGGERS_BOARD
        // =========================================================
        private async void Bt_Triggers_Board_Click(object sender, RoutedEventArgs e)
        {
            //Vider_Historiques();
            if (bTriggersBoard_Actif)
            {
                StopTousModes();

                return;
            }

            StopTousModes();
            await Task.Delay(50);

            bTriggersBoard_Actif = true;

            // =====================================
            // Autoriser l'écriture dans l'historique
            // =====================================
            bAutoriserHistorique = true;

            Bt_Triggers_Board.Background = Brushes.LightGreen;

            bool bMain1_Lu = false;
            bool bMain2_Lu = false;
            bool bFlop1_Lu = false;
            bool bFlop2_Lu = false;
            bool bFlop3_Lu = false;
            bool bTurn_Lu = false;
            bool bRiver_Lu = false;
            bool bGris = false;
            // =====================================
            // Nouvelle session
            // =====================================

            RTB_Historique.Document.Blocks.Clear();

            while (bTriggersBoard_Actif)
            {
                if (JeuEnPause)
                {
                    await Task.Delay(100);
                    continue;
                }
                await Task.Delay(1);

                CaptureEcran();

                if (Pupitre_Gris_Detecte())
                {
                    bGris = true;

                    // ==========================================
                    // PLUS DE POT
                    // ==========================================
                    if (Plus_De_Pot_Detecte())
                    {
                        await Task.Delay(5200);

                        CaptureEcran();

                        if (Plus_De_Jetons_Detecte())
                        {
                            Se_Recaver();
                            continue;
                        }
                    }
                }

                // ==========================
                // RETOUR DEBUT
                // ==========================
                if (bGris && !Pupitre_Gris_Detecte())
                {
                    bGris = false;

                    // ==========================
                    // HISTORIQUE
                    // ==========================
                    if (bAutoriserHistorique && TB_M1.Text != "")
                    {
                        Ajouter_Historique();

                        NbMains += 1;
                        LbNbParole.Content = NbMains.ToString();
                    }

                    // ==========================
                    // REMISE A ZERO
                    // ==========================
                    bMain1_Lu = false;
                    bMain2_Lu = false;

                    bFlop1_Lu = false;
                    bFlop2_Lu = false;
                    bFlop3_Lu = false;

                    bTurn_Lu = false;
                    bRiver_Lu = false;

                    Reset_Table();

                    continue;
                }

                // ==========================
                // MAIN 1
                // ==========================
                if (!bMain1_Lu &&
                    Detecter_Presence("Main_1_Presence"))
                {
                    Lire_Main_1();

                    bMain1_Lu = true;

                    continue;
                }

                // ==========================
                // MAIN 2
                // ==========================
                if (bMain1_Lu &&
                    !bMain2_Lu &&
                    Detecter_Presence("Main_2_Presence"))
                {
                    Lire_Main_2();

                    bMain2_Lu = true;

                    Calcul_PreFlop();

                    continue;
                }
                // ==========================
                // FLOP 1
                // ==========================
                if (bMain2_Lu && !bFlop1_Lu && Detecter_Presence("Flop_1_Presence"))
                {
                    Lire_Flop_1();

                    bFlop1_Lu = true;

                    continue;
                }
                // ==========================
                // FLOP 2
                // ==========================
                if (bFlop1_Lu && !bFlop2_Lu &&
                    Detecter_Presence("Flop_2_Presence"))
                {
                    Lire_Flop_2();

                    bFlop2_Lu = true;

                    continue;
                }
                // ==========================
                // FLOP 3
                // ==========================
                if (bFlop2_Lu && !bFlop3_Lu &&
                    Detecter_Presence("Flop_3_Presence"))
                {
                    Lire_Flop_3();

                    bFlop3_Lu = true;

                    Calcul_Flop();
                    Calcul_Tirages_Flop();

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
                        Lire_Turn();

                        bTurn_Lu = true;

                        Calcul_Turn();
                        Calcul_Tirages_Turn();

                        continue;
                    }

                    // ----- Turn HB -----
                    if (Detecter_Presence("Turn_HB_Presence"))
                    {
                        Lire_Turn_HB();

                        bTurn_Lu = true;

                        Calcul_Turn();
                        Calcul_Tirages_Turn();

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

                        bRiver_Lu = true;

                        Calcul_River();

                        continue;
                    }

                    // ----- River HB -----
                    if (Detecter_Presence("River_HB_Presence"))
                    {
                        Lire_River_HB();

                        bRiver_Lu = true;

                        Calcul_River();

                        continue;
                    }
                }
            }

            Bt_Triggers_Board.Background = Brushes.White;
        }

        // =========================================================
        // TAPIS PREFLOP
        // =========================================================
        private async void Bt_Tapis_PreFlop_Click(object sender, RoutedEventArgs e)
        {
            //Vider_Historiques();
            if (bTapisPreFlop_Actif)
            {
                StopTousModes();

                return;
            }

            StopTousModes();
            Reset_Table();
            await Task.Delay(50);

            bTapisPreFlop_Actif = true;

            // =====================================
            // Autoriser l'écriture dans l'historique
            // =====================================
            bAutoriserHistorique = true;

            Bt_Tapis_PreFlop.Background = Brushes.LightGreen;

            bool bMain1_Lu = false;
            bool bMain2_Lu = false;
            bool bFlop1_Lu = false;
            bool bFlop2_Lu = false;
            bool bFlop3_Lu = false;
            bool bTurn_Lu = false;
            bool bRiver_Lu = false;
            bool bGris = false;
            // =====================================
            // Nouvelle session
            // =====================================

            RTB_Historique.Document.Blocks.Clear();

            while (bTapisPreFlop_Actif)
            {
                if (JeuEnPause)
                {
                    await Task.Delay(100);
                    continue;
                }
                await Task.Delay(1);

                CaptureEcran();

                if (Pupitre_Gris_Detecte())
                {
                    bGris = true;

                    //MessageBox.Show("bGris = true");

                    // ==========================================
                    // PLUS DE POT
                    // ==========================================
                    if (Plus_De_Pot_Detecte())
                    {
                        await Task.Delay(5200);

                        CaptureEcran();

                        if (Plus_De_Jetons_Detecte())
                        {
                            Se_Recaver();
                            continue;
                        }
                    }
                }

                // ==========================
                // RETOUR DEBUT
                // ==========================
                if (bGris && !Pupitre_Gris_Detecte())
                {
                    bGris = false;

                    //MessageBox.Show("bGris = false");

                    // ==========================
                    // HISTORIQUE
                    // ==========================
                    if (bAutoriserHistorique && bMain1_Lu &&  bMain2_Lu)
                    {
                        Ajouter_Historique();

                        NbMains += 1;
                        LbNbParole.Content = NbMains.ToString();
                    }

                    // ==========================
                    // REMISE A ZERO
                    // ==========================
                    bMain1_Lu = false;
                    bMain2_Lu = false;

                    bFlop1_Lu = false;
                    bFlop2_Lu = false;
                    bFlop3_Lu = false;

                    bTurn_Lu = false;
                    bRiver_Lu = false;

                    Reset_Table();

                    continue;
                }

                // ==========================
                // MAIN 1
                // ==========================
                if (!bMain1_Lu &&
                    Detecter_Presence("Main_1_Presence"))
                {
                    Lire_Main_1();

                    bMain1_Lu = true;

                    continue;
                }

                // ==========================
                // MAIN 2
                // ==========================
                if (bMain1_Lu &&
                    !bMain2_Lu &&
                    Detecter_Presence("Main_2_Presence"))
                {
                    Lire_Main_2();

                    bMain2_Lu = true;

                    Calcul_PreFlop();

                    continue;
                }
                // ==========================
                // FLOP 1
                // ==========================
                if (bMain2_Lu && !bFlop1_Lu && Detecter_Presence("Flop_1_Presence"))
                {
                    Lire_Flop_1();

                    bFlop1_Lu = true;

                    continue;
                }
                // ==========================
                // FLOP 2
                // ==========================
                if (bFlop1_Lu && !bFlop2_Lu &&
                    Detecter_Presence("Flop_2_Presence"))
                {
                    Lire_Flop_2();

                    bFlop2_Lu = true;

                    continue;
                }
                // ==========================
                // FLOP 3
                // ==========================
                if (bFlop2_Lu && !bFlop3_Lu &&
                    Detecter_Presence("Flop_3_Presence"))
                {
                    Lire_Flop_3();

                    bFlop3_Lu = true;

                    Calcul_Flop();
                    Calcul_Tirages_Flop();

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
                        Lire_Turn();

                        bTurn_Lu = true;

                        Calcul_Turn();
                        Calcul_Tirages_Turn();

                        continue;
                    }

                    // ----- Turn HB -----
                    if (Detecter_Presence("Turn_HB_Presence"))
                    {
                        Lire_Turn_HB();

                        bTurn_Lu = true;

                        Calcul_Turn();
                        Calcul_Tirages_Turn();

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

                        bRiver_Lu = true;

                        Calcul_River();

                        continue;
                    }

                    // ----- River HB -----
                    if (Detecter_Presence("River_HB_Presence"))
                    {
                        Lire_River_HB();

                        bRiver_Lu = true;

                        Calcul_River();

                        continue;
                    }
                }
            }

            Bt_Tapis_PreFlop.Background = Brushes.White;
        }

        private void Fusion_Historique()
        {
            MessageBox.Show("Fusion Historique");
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

        // =========================================================
        // 🔥 STOP TOUS MODES
        // =========================================================
        private void StopTousModes()
        {
            // =====================================
            // Arrêt de tous les modes
            // =====================================
            // Ne plus autoriser l'ajout de nouvelles mains
            bAutoriserHistorique = false;

            bTestBoard = false;

            bTapisPreFlop = false;

            if (bTriggersBoard_Actif)
            {
                bTriggersBoard_Actif = false;

                // =====================================
                // Ajouter la dernière main affichée
                // =====================================

                Ajouter_Historique();

                Sauvegarder_Historique_TXT();

                Sauvegarder_Historique_RTF();

                Sauvegarder_Historique_JSON();
            }

            // =====================================
            // Remise à zéro des boutons
            // =====================================

            Bt_Test_Board.Background = Brushes.White;

            Bt_Triggers_Board.Background = Brushes.White;

            Bt_Tapis_PreFlop.Background = Brushes.White;

            Reset_Table();
        }

        #endregion

    }
}
