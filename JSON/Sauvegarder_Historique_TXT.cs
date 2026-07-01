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