using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Poker
{
    public partial class MainWindow : Window
    {
        // =====================================================
        // STRUCTURE DETECTION CARTE
        // =====================================================

        public struct S_CarteDetect
        {
            public string Nom;

            public int X;
            public int Y;

            public int Couleur;
        }
        // =====================================================
        // TABLEAU MAIN1
        // =====================================================

        private S_CarteDetect[] Main1Detect =
            new S_CarteDetect[52];


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
        private bool bPreFlop_Paire,   bPreFlop_Suite,    bPreFlop_Pareille;
        private bool bFlop_Paire,      bFlop_2_Paires,    bFlop_Brelan,     bFlop_Quinte,      bFlop_Couleur,    bFlop_Full,      bFlop_Carre,      bFlop_QFlush;
        private bool bTurn_Paire,      bTurn_2_Paires,    bTurn_Brelan,     bTurn_Quinte,      bTurn_Couleur,    bTurn_Full,      bTurn_Carre,      bTurn_QFlush;
        private bool bRiver_Paire,     bRiver_2_Paires,   bRiver_Brelan,    bRiver_Quinte,     bRiver_Couleur,   bRiver_Full,     bRiver_Carre,     bRiver_QFlush;
        
        private bool bSe_Recare;
        private bool NM9, NM8, NM6, NM2, NM0, NP2, NP4, NP6, NP8, NP10;
        private bool bPosition_1, bPosition_2, bPosition_3, bPosition_4, bPosition_5, bPosition_6, bPosition_7, bPosition_8, bPosition_9;
        private bool bGrenhorn, bBeginner, bGambler, bPro, bBigFish, bMaster, bChampion, bGuru, bUltimate, bEpic, bInsane, bLegendary ;

        private bool ss, bBouton_en_1, bBouton_en_2, bBouton_en_3, bBouton_en_4, bBouton_en_5, bBouton_en_6, bBouton_en_7, bBouton_en_8, bBouton_en_9;

        private int NbDoublon_Flop1;
        private int NbDoublon_Flop2;
        private int NbDoublon_Flop3;

        private int NbDoublon_Turn;
        private int NbDoublon_River;




        HashSet<string> RangeTapis =            new HashSet<string>();
        HashSet<string> RangeRelance =          new HashSet<string>();
        HashSet<string> RangeCall =             new HashSet<string>();
        HashSet<string> RangeFold =             new HashSet<string>();
        Dictionary<string, Button> BoutonsRange;
        List<Button> BoutonsAutoMise = new List<Button>();
        List<Button> BoutonsMemo =              new List<Button>();
        List<Button> BoutonsNiveauTable =       new List<Button>();
        List<Button> BoutonsMise =              new List<Button>();
        List<Button> BoutonsPosition =          new List<Button>();
        List<Button> BoutonsPlaceDisponible =   new List<Button>();
        List<Button> BoutonsBoutonPosition =    new List<Button>();

        List<Button> BoutonsJoueur_Mise =       new List<Button>();
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

        private readonly string[] Valeurs =
        {
            "A","K","Q","J","10","9","8","7","6","5","4","3","2"
        };

        private readonly string[] Symboles =
        {
            "♣","♦","♥","♠"
        };

        // Générateur aléatoire
        Random rnd = new Random();

        // Paquet de cartes (0..51)
        List<int> Paquet = new List<int>();

        public MainWindow()
        {
            InitializeComponent();

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
            Bt_Mem07_Click(null, null);

            if (Bt_Se_Recare.Background.Equals((Brush)FindResource("blanc")))
            {
                Bt_Se_Recare_Click(null, null);
            }
            // 👉 Appliquer Fin Main au démarrage
            Bt_Fin_Main_Click(null, null);

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
            public int Numero,N;   // 0..51
            public int Couleur,C;  // 0..3
            public int Valeur,V;   // 1..13 (2..As)
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
        private void AfficherCarte(
            TextBox tb,
            Button precedent,
            Button courant,
            int indexCarte)
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
        //public Brush Bt_Pas_de_Bouton { get; private set; }


        private void Initialise_Boutons_Main_Flop_Turn_River()
        {
            Bt_Main_1.Background = Orange;
            Bt_Main_2.Background = Blanc;
            Bt_Flop_1.Background = Blanc;
            Bt_Flop_2.Background = Blanc;
            Bt_Flop_3.Background = Blanc;
            Bt_Turn.Background = Blanc;
            Bt_River.Background = Blanc;
            Bt_Fin_Main.Background = Blanc;
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
        private bool Extraire_Main1(string txt)
        {
            string[] lignes =
                txt.Split('\n');

            int index = 0;

            bool bMain1 = false;

            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne =
                    lignes[i].Trim();

                // =====================================
                // COMMENT : Main 1
                // =====================================

                if (ligne == "COMMENT : Main 1")
                {
                    bMain1 = true;
                    continue;
                }

                // =====================================
                // COMMENT : Carte
                // =====================================

                if (ligne.StartsWith("COMMENT :"))
                {
                    string nom =
                        ligne.Replace(
                            "COMMENT :",
                            "")
                            .Trim();

                    // ignorer lignes invalides

                    if (nom == "Main 1")
                        continue;

                    if (nom.Length < 2)
                        continue;

                    // =====================================
                    // Ligne suivante :
                    // IF PIXEL COLOR EQUALS
                    // =====================================

                    if (i + 1 >= lignes.Length)
                        continue;

                    string l2 =
                        lignes[i + 1].Trim();

                    if (!l2.StartsWith(
                        "IF PIXEL COLOR EQUALS"))
                        continue;

                    // =====================================
                    // Découpage :
                    // IF PIXEL COLOR EQUALS : X : Y : C : 0
                    // =====================================

                    string[] t =
                        l2.Split(':');

                    if (t.Length < 5)
                        continue;

                    // =====================================
                    // Extraction
                    // =====================================

                    Main1Detect[index].Nom =
                        nom;

                    Main1Detect[index].X =
                        int.Parse(t[1]);

                    Main1Detect[index].Y =
                        int.Parse(t[2]);

                    Main1Detect[index].Couleur =
                        int.Parse(t[3]);

                    index++;
                }
            }

            // =====================================
            // Validation
            // =====================================

            if (!bMain1)
                return false;

            if (index != 52)
                return false;

            return true;
        }
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
            Efface_Affichage_Cartes();
            Initialise_Boutons_Main_Flop_Turn_River();

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
        /************************************
        **                                 **
        **              F L O P            **
        **                                 **
        ************************************/
        private void Bt_Flop_1_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte(TB_F1, Bt_Main_2, Bt_Flop_1, 2);
            Verifier_Doublon_Flop1();
        }

        private void Bt_Flop_2_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte(TB_F2, Bt_Flop_1, Bt_Flop_2, 3);
            Verifier_Doublon_Flop2();
        }

        private void Bt_Flop_3_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte(TB_F3, Bt_Flop_2, Bt_Flop_3, 4);
            Verifier_Doublon_Flop2();

            // 🔥 Analyse complète du flop
            Calcul_Flop();
        }
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
        /************************************
        **                                 **
        **              T U R N            **
        **                                 **
        ************************************/

        private void Bt_Turn_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte(TB_T, Bt_Flop_3, Bt_Turn, 5);


            // 🔥 Analyse Turn
            Calcul_Turn();
        }
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
            // 🔥 TIRAGE COULEUR
            // 4 cartes même couleur
            // =========================================================

            bool tirCouleur =
                couleurs.GroupBy(c => c)
                        .Any(g => g.Count() == 4);

            if (tirCouleur)
            {
                bTir_Turn_Couleur = true;
                Bt_Tir_Turn_Couleur.Background = Vert;
            }

            // =========================================================
            // 🔥 TIRAGE QUINTE
            // 4 cartes qui se suivent
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

                    if (suite == 4)
                    {
                        bTir_Turn_Quinte = true;
                        Bt_Tir_Turn_Quinte.Background = Vert;
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

            if (bTir_Turn_Quinte && bTir_Turn_Couleur)
            {
                bTir_Turn_QFlush = true;
                Bt_Tir_Turn_QFlush.Background = Vert;
            }
        }
        /************************************
        **                                 **
        **             R I V E R           **
        **                                 **
        ************************************/

        private void Bt_River_Click(object sender, RoutedEventArgs e)
        {
            AfficherCarte(TB_R, Bt_Turn, Bt_River, 6);

            // 🔥 Analyse River
            Calcul_River();
        }
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
        private Brush CouleurCarte(string c)
        {
            if (c.Contains("♣")) return Brushes.Blue;
            if (c.Contains("♦")) return Brushes.Green;
            if (c.Contains("♥")) return Brushes.Red;
            if (c.Contains("♠")) return Brushes.Black;
            return Brushes.Black;
        }

        private void Fermer_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Bt_Fin_Main_Click(object sender, RoutedEventArgs e)
        {
            if (Bt_Main_1.Background.Equals((Brush)FindResource("orange"))) return;// imposible de faire Fin_Main sur main 1
            if (Bt_Flop_1.Background.Equals((Brush)FindResource("orange"))) return;// imposible de faire Fin_Main sur Flop 1
            if (Bt_Flop_2.Background.Equals((Brush)FindResource("orange"))) return;// imposible de faire Fin_Main sur Flop 2
            PaquetInitialise = false;
            Bt_Fin_Main.Background = Orange;
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
            Bt_Fin_Main.Background = Blanc;
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

            Bt_Fin_Main.Background = Blanc;

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

            Bt_Fin_Main.Background = Blanc;

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

            Bt_Fin_Main.Background = Blanc;

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

            Bt_Fin_Main.Background = Blanc;

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

        private void Bt_Joueur1_Fold_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur2_Fold_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur3_Fold_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur4_Fold_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur5_Fold_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur6_Fold_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur7_Fold_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur8_Fold_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur2_Tapis_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur9_Fold_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur3_Tapis_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur4_Tapis_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur5_Tapis_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur6_Tapis_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur7_Tapis_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur8_Tapis_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur9_Tapis_Click(object sender, RoutedEventArgs e){}
        private void Bt_Gagne_en_1_Click(object sender, RoutedEventArgs e){}
        private void Bt_Gagne_en_2_Click(object sender, RoutedEventArgs e){}
        private void Bt_Gagne_en_3_Click(object sender, RoutedEventArgs e){}
        private void Bt_Gagne_en_4_Click(object sender, RoutedEventArgs e){}
        private void Bt_Gagne_en_5_Click(object sender, RoutedEventArgs e){}
        private void Bt_Gagne_en_6_Click(object sender, RoutedEventArgs e){}
        private void Bt_Gagne_en_7_Click(object sender, RoutedEventArgs e){}
        private void Bt_Gagne_en_8_Click(object sender, RoutedEventArgs e){}
        private void Bt_Gagne_en_9_Click(object sender, RoutedEventArgs e){}
        private void Bt_Tapis_PreFlop_Click(object sender, RoutedEventArgs e){}
        private void Bt_Force_Parole_Passe_Click(object sender, RoutedEventArgs e){}
        private void Bt_J_ai_Fais_Tapis_Click(object sender, RoutedEventArgs e){}
        private void Bt_Je_Fais_Tapis_Click(object sender, RoutedEventArgs e){}
        private void Bt_Bouton_Click(object sender, RoutedEventArgs e){}
        private void Bt_S_Click(object sender, RoutedEventArgs e){}
        private void tB_Mise_Coeff_TextChanged(object sender, TextChangedEventArgs e){}
        private void tB_Mise_TextChanged(object sender, TextChangedEventArgs e){}
        private void Bt_PreFlop_Paire_Click(object sender, RoutedEventArgs e){}
        private void Bt_PreFlop_Pareille_Click(object sender, RoutedEventArgs e){}
        private void Bt_PreFlop_Suite_Click(object sender, RoutedEventArgs e){}
        private void Bt_Flop_Paire_Click(object sender, RoutedEventArgs e){}
        private void Bt_Flop_2_Paires_Click(object sender, RoutedEventArgs e){}
        private void Bt_Flop_Brelan_Click(object sender, RoutedEventArgs e){}
        private void Bt_Flop_Quinte_Click(object sender, RoutedEventArgs e){}
        private void Bt_Flop_Couleur_Click(object sender, RoutedEventArgs e){}
        private void Bt_Flop_Full_Click(object sender, RoutedEventArgs e){}
        private void Bt_Flop_Carre_Click(object sender, RoutedEventArgs e){}
        private void Bt_Flop_QFlush_Click(object sender, RoutedEventArgs e){}
        private void Bt_Turn_Paire_Click(object sender, RoutedEventArgs e){}
        private void Bt_Turn_2_Paires_Click(object sender, RoutedEventArgs e){}
        private void Bt_Turn_Brelan_Click(object sender, RoutedEventArgs e){}
        private void Bt_Turn_Quinte_Click(object sender, RoutedEventArgs e){}
        private void Bt_Turn_Couleur_Click(object sender, RoutedEventArgs e){}
        private void Bt_Turn_Full_Click(object sender, RoutedEventArgs e){}
        private void Bt_Turn_Carre_Click(object sender, RoutedEventArgs e){}
        private void Bt_Turn_QFlush_Click(object sender, RoutedEventArgs e){}
        private void Bt_River_Paire_Click(object sender, RoutedEventArgs e){}
        private void Bt_River_2_Paires_Click(object sender, RoutedEventArgs e){}
        private void Bt_River_Brelan_Click(object sender, RoutedEventArgs e){}
        private void Bt_River_Quinte_Click(object sender, RoutedEventArgs e){}
        private void Bt_River_Couleur_Click(object sender, RoutedEventArgs e){}
        private void Bt_River_Full_Click(object sender, RoutedEventArgs e){}
        private void Bt_River_Carre_Click(object sender, RoutedEventArgs e){}
        private void Bt_River_QFlush_Click(object sender, RoutedEventArgs e){}
        private void Bt_Tir_Flop_Quinte_Click(object sender, RoutedEventArgs e){}
        private void Bt_Tir_Flop_Couleur_Click(object sender, RoutedEventArgs e){}
        private void Bt_Tir_Flop_QFlush_Click(object sender, RoutedEventArgs e){}
        private void Bt_Tir_Turn_Quinte_Click(object sender, RoutedEventArgs e){}
        private void Bt_Tir_Turn_Couleur_Click(object sender, RoutedEventArgs e){}
        private void Bt_Tir_Turn_QFlush_Click(object sender, RoutedEventArgs e){}
        private void Bt_Suivre_Click(object sender, RoutedEventArgs e){}
        private void Bt_Passe_Click(object sender, RoutedEventArgs e){}
        private void Bt_Parole_Click(object sender, RoutedEventArgs e){}
        private void Bt_Relance_Click(object sender, RoutedEventArgs e){}
        private void Bt_Joueur1_Tapis_Click(object sender, RoutedEventArgs e){}
        private void boutonRAZ_Click(object sender, RoutedEventArgs e){}

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

        /************************************
        **                                 **
        **              M E M O            **
        **                                 **
        ************************************/
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
            //Bt_Pas_de_Bouton.Background = (Brush)FindResource("blanc");
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
            LbNbParole.Content = NbParole.ToString();
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

        private void Bt_Initialise_Click(object sender, RoutedEventArgs e)
        {
            // 🔥 Remise à zéro des compteurs
            NbMains = 0;
            NbTapis = 0;
            NbParole = 0;

            // 🔥 Reset du temps
            DebutSession = DateTime.Now;

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
        private void Verifier_Doublon_Flop1()
        {
            Bt_Doublon.Background = Blanc;

            if (TB_F1.Text == TB_M2.Text)
            {
                Bt_Doublon.Background = Bt_Doublon.Background = (Brush)FindResource("rouge");

                NbDoublon_Flop1++;
            }
        }
        private void Verifier_Doublon_Flop2()
        {
            Bt_Doublon.Background = Blanc;

            if (TB_F2.Text == TB_F1.Text)
            {
                Bt_Doublon.Background = Bt_Doublon.Background = (Brush)FindResource("rouge");

                NbDoublon_Flop2++;
            }
        }
        private void Verifier_Doublon_Flop3()
        {
            Bt_Doublon.Background = Blanc;

            if (TB_F3.Text == TB_F2.Text)
            {
                Bt_Doublon.Background = Bt_Doublon.Background = (Brush)FindResource("rouge");

                NbDoublon_Flop3++;
            }
        }
        private void Verifier_Doublon_Turn()
        {
            Bt_Doublon.Background = Blanc;

            if (TB_T.Text == TB_F3.Text)
            {
                Bt_Doublon.Background = Bt_Doublon.Background = (Brush)FindResource("rouge"); 

                NbDoublon_Turn++;
            }
        }
        private void Verifier_Doublon_River()
        {
            Bt_Doublon.Background = Blanc;

            if (TB_R.Text == TB_T.Text)
            {
                Bt_Doublon.Background = Bt_Doublon.Background = (Brush)FindResource("rouge");

                NbDoublon_River++;
            }
        }
        private void Bt_Injection_Click(object sender, RoutedEventArgs e)
        {
            // =====================================
            // Lecture presse-papiers
            // =====================================

            string txt =
                Clipboard.GetText();

            // =====================================
            // Rien ?
            // =====================================

            if (string.IsNullOrWhiteSpace(txt))
            {
                MessageBox.Show(
                    "Aucune donnée exploitable");

                return;
            }

            // =====================================
            // Affichage dans TextBox
            // =====================================

            TB_Injection.Text = txt;

            // =====================================
            // Extraction Main1
            // =====================================

            bool ok =
                Extraire_Main1(txt);

            // =====================================
            // Vérification
            // =====================================

            if (!ok)
            {
                MessageBox.Show(
                    "Aucune donnée exploitable");

                return;
            }

            MessageBox.Show("Injection OK\n\n" + "52 cartes Main1 reçues");
        }
    }
}