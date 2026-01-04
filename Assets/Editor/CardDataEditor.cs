using UnityEngine;
using UnityEditor;
using static CardData;
using static UnityEngine.InputSystem.InputRemoting;

[CustomEditor(typeof(CardData))]
public class CardDataEditor : Editor
{
    private SerializedProperty cardNameProp;
    private SerializedProperty cardBaseNameProp;
    private SerializedProperty iconProp;
    private SerializedProperty cauldronContentsSpriteProp;
    private SerializedProperty cardColorProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty cardPrefabProp;
    private SerializedProperty itemTypeProp;
    private SerializedProperty partIconProp;
    private SerializedProperty canBeSoldProp;

    private SerializedProperty incineratesProp;
    private SerializedProperty smouldersProp;
    private SerializedProperty toxinProp;
    private SerializedProperty antidoteProp;
    private SerializedProperty neutralProp;

    private SerializedProperty edibleProp;
    private SerializedProperty sweetProp;
    private SerializedProperty bitterProp;
    private SerializedProperty saltyProp;
    private SerializedProperty spicyProp;
    private SerializedProperty floweryProp;
    private SerializedProperty umamiProp;

    private SerializedProperty shelfVisualsProp;
    private SerializedProperty processingRecipesProp;

    private void OnEnable()
    {
        cardNameProp = serializedObject.FindProperty("cardName");
        cardBaseNameProp = serializedObject.FindProperty("baseName");
        iconProp = serializedObject.FindProperty("Icon");
        cauldronContentsSpriteProp = serializedObject.FindProperty("cauldronContentsSprite");
        cardColorProp = serializedObject.FindProperty("cardColor");
        descriptionProp = serializedObject.FindProperty("description");
        cardPrefabProp = serializedObject.FindProperty("cardPrefab");
        itemTypeProp = serializedObject.FindProperty("itemType");
        partIconProp = serializedObject.FindProperty("partIcon");
        canBeSoldProp = serializedObject.FindProperty("canBeSold");

        incineratesProp = serializedObject.FindProperty("Incinerates");
        smouldersProp = serializedObject.FindProperty("Smoulders");
        toxinProp = serializedObject.FindProperty("Toxin");
        antidoteProp = serializedObject.FindProperty("Antidote");
        neutralProp = serializedObject.FindProperty("Neutral");

        edibleProp = serializedObject.FindProperty("Edible");
        sweetProp = serializedObject.FindProperty("Sweet");
        bitterProp = serializedObject.FindProperty("Bitter");
        saltyProp = serializedObject.FindProperty("Salty");
        spicyProp = serializedObject.FindProperty("Spicy");
        floweryProp = serializedObject.FindProperty("Flowery");
        umamiProp = serializedObject.FindProperty("Umami");

        shelfVisualsProp = serializedObject.FindProperty("shelfVisuals");

        processingRecipesProp = serializedObject.FindProperty("processingRecipes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // --- Main card fields ---
        EditorGUILayout.PropertyField(cardNameProp);
        EditorGUILayout.PropertyField(cardBaseNameProp);
        EditorGUILayout.PropertyField(iconProp);
        EditorGUILayout.PropertyField(cauldronContentsSpriteProp);
        EditorGUILayout.PropertyField(cardColorProp);
        EditorGUILayout.PropertyField(descriptionProp);
        EditorGUILayout.PropertyField(cardPrefabProp);
        EditorGUILayout.PropertyField(itemTypeProp);
        EditorGUILayout.PropertyField(partIconProp);

        EditorGUILayout.Space();
        //EditorGUILayout.LabelField("Selling", EditorStyles.boldLabel);

        // Implicit sellability hint (read-only, informational)
        ProcessedType processedType = ((CardData)target).processedType;
        bool implicitlySellable =
            processedType == ProcessedType.Potion ||
            processedType == ProcessedType.Poison;

        EditorGUILayout.BeginVertical("box");

        if (implicitlySellable)
        {
            EditorGUILayout.HelpBox(
                $"This card is implicitly sellable because it is a {processedType}.",
                UnityEditor.MessageType.Info
            );
        }

        // Explicit override
        EditorGUILayout.PropertyField(
            canBeSoldProp,
            new GUIContent("Can Be Sold")
        );

        // OR change above 4 lines to below if you want to prevent the sale of raw ingredients
        //bool isProcessed = ((CardData)target).processedType != ProcessedType.None;

        //GUI.enabled = isProcessed;
        //EditorGUILayout.PropertyField(canBeSoldProp);
        //GUI.enabled = true;
        //);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);

        // Incinerates / Smoulders mutually exclusive
        EditorGUILayout.PropertyField(incineratesProp);
        if (incineratesProp.boolValue) smouldersProp.boolValue = false;
        GUI.enabled = !incineratesProp.boolValue;
        EditorGUILayout.PropertyField(smouldersProp);
        GUI.enabled = true;

        // Edible toggle & flavors
        bool prevEdible = edibleProp.boolValue;
        EditorGUILayout.PropertyField(edibleProp);
        if (!edibleProp.boolValue && prevEdible)
        {
            toxinProp.boolValue = false;
            antidoteProp.boolValue = false;
            neutralProp.boolValue = false;
            umamiProp.boolValue = false;
            sweetProp.boolValue = bitterProp.boolValue = saltyProp.boolValue =
            spicyProp.boolValue = floweryProp.boolValue = false;
        }

        // Draw Toxicity
        bool canEdit = edibleProp.boolValue;
        bool toxinOn = toxinProp.boolValue;
        bool antidoteOn = antidoteProp.boolValue;
        bool neutralOn = neutralProp.boolValue;

        int selectedCount = (toxinOn ? 1 : 0) + (antidoteOn ? 1 : 0) + (neutralOn ? 1 : 0);

        GUI.enabled = canEdit && (selectedCount == 0 || toxinOn);
        EditorGUILayout.PropertyField(toxinProp);
        GUI.enabled = canEdit && (selectedCount == 0 || antidoteOn);
        EditorGUILayout.PropertyField(antidoteProp);
        GUI.enabled = canEdit && (selectedCount == 0 || neutralOn);
        EditorGUILayout.PropertyField(neutralProp);
        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Flavour Profile", EditorStyles.boldLabel);

        int selectedFlavors = 0;
        if (sweetProp.boolValue) selectedFlavors++;
        if (bitterProp.boolValue) selectedFlavors++;
        if (saltyProp.boolValue) selectedFlavors++;
        if (spicyProp.boolValue) selectedFlavors++;
        if (floweryProp.boolValue) selectedFlavors++;

        bool prevUmami = umamiProp.boolValue;
        EditorGUILayout.PropertyField(umamiProp);
        if (umamiProp.boolValue && !prevUmami)
        {
            sweetProp.boolValue = bitterProp.boolValue = saltyProp.boolValue =
            spicyProp.boolValue = floweryProp.boolValue = false;
        }

        GUI.enabled = edibleProp.boolValue && !umamiProp.boolValue && selectedFlavors < 2 || sweetProp.boolValue;
        EditorGUILayout.PropertyField(sweetProp);
        GUI.enabled = edibleProp.boolValue && !umamiProp.boolValue && selectedFlavors < 2 || bitterProp.boolValue;
        EditorGUILayout.PropertyField(bitterProp);
        GUI.enabled = edibleProp.boolValue && !umamiProp.boolValue && selectedFlavors < 2 || saltyProp.boolValue;
        EditorGUILayout.PropertyField(saltyProp);
        GUI.enabled = edibleProp.boolValue && !umamiProp.boolValue && selectedFlavors < 2 || spicyProp.boolValue;
        EditorGUILayout.PropertyField(spicyProp);
        GUI.enabled = edibleProp.boolValue && !umamiProp.boolValue && selectedFlavors < 2 || floweryProp.boolValue;
        EditorGUILayout.PropertyField(floweryProp);
        GUI.enabled = true;

        EditorGUILayout.Space();
        if (GUILayout.Button("Apply Default Color"))
        {
            ((CardData)target).ApplyDefaultColor();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Shelf Visuals (Spell Outputs)", EditorStyles.boldLabel);

        CardData card = (CardData)target;

        // This is the real rule:
        bool usesSpellOutputPrefab =
            card.cardPrefab != null &&
            card.cardPrefab.GetComponent<SpellOutput>() != null;

        EditorGUILayout.BeginVertical("box");

        if (!usesSpellOutputPrefab)
        {
            EditorGUILayout.HelpBox(
                "Shelf visuals are only used by cards that instantiate as SpellOutput (sellable brews).",
                UnityEditor.MessageType.Info
            );
        }

        GUI.enabled = usesSpellOutputPrefab;
        EditorGUILayout.PropertyField(
            shelfVisualsProp,
            new GUIContent("Shelf Visual Data")
        );
        GUI.enabled = true;

        if (usesSpellOutputPrefab && shelfVisualsProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox(
                "This SpellOutput card will appear on shop shelves but has no shelf visuals assigned.",
                UnityEditor.MessageType.Warning
            );
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Processing Recipes", EditorStyles.boldLabel);

        for (int i = 0; i < processingRecipesProp.arraySize; i++)
        {
            SerializedProperty recipeProp = processingRecipesProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.PropertyField(recipeProp.FindPropertyRelative("tool"));
            EditorGUILayout.PropertyField(recipeProp.FindPropertyRelative("resultCard"));
            EditorGUILayout.PropertyField(recipeProp.FindPropertyRelative("processingTime"));
            EditorGUILayout.PropertyField(recipeProp.FindPropertyRelative("processedResultType"));

            SerializedProperty needsFireProp = recipeProp.FindPropertyRelative("needsFire");
            EditorGUILayout.PropertyField(needsFireProp);
            if (needsFireProp.boolValue)
            {
                EditorGUILayout.PropertyField(recipeProp.FindPropertyRelative("processingTimeWithFire"));
            }

            // Visual outputs
            SerializedProperty outputsProp = recipeProp.FindPropertyRelative("visualOutputs");
            EditorGUILayout.PropertyField(outputsProp, new GUIContent("Visual Outputs"), true);

            // Remove recipe button
            if (GUILayout.Button("Remove Recipe"))
            {
                processingRecipesProp.DeleteArrayElementAtIndex(i);
                i--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add New Recipe"))
        {
            processingRecipesProp.arraySize++;
        }

        serializedObject.ApplyModifiedProperties();
    }
}