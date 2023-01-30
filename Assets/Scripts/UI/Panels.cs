using UnityEngine;

public static class Panels
{
    private static GameObject activePanel;

    public static void SetActivePanel( GameObject panel )
    {
        if ( activePanel != null )
        {
            activePanel.SetActive( false );
        }
        activePanel = panel;
        activePanel.SetActive( true );
    }
}