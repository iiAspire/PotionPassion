using UnityEngine;

public class ComboTester : MonoBehaviour
{
    public ComboGenerator comboGenerator;

    void Start()
    {
        if (comboGenerator == null)
        {
            Debug.LogError("ComboTester: ComboGenerator not assigned.");
            return;
        }

        SpellCombo testCombo = comboGenerator.GenerateCombo(
            spellName: "TEST_SPELL",
            ingredientCount: 3,
            needsIntimate: true,
            needsSpiritual: false,
            needsAstrological: false,
            needsElement: false,
            needsTool: false
        );

        Debug.Log("=== ComboTester Result ===");
        Debug.Log(testCombo.SpellName);
        Debug.Log(string.Join(", ", testCombo.Ingredients));
    }
}