using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


    public class DialogueParser : MonoBehaviour
    {
        [Range(0.01f, 0.2f)]
        public float m_characterInterval;

        private float m_cumalativeDeltaTime;

        private float m_tempInterval;

        public bool isWtritting;
        public bool buttonAllwaysActiving;

        [TextArea] 
        public string temperalText;

        private string m_text;

        [SerializeField] private DialogueContainer dialogue;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI dialogueChar;

        [SerializeField] private Button choicePrefab;
        [SerializeField] private Transform buttonContainer;

        public GameObject buttonsContainer;


        [Header("Special Node ID Activator")]
        public bool SpecialId;
        [TextArea] public string DialogueNodeID; 



        private void Start()
        {
            m_tempInterval = m_characterInterval;

            if (!buttonAllwaysActiving)
            {
                buttonsContainer.SetActive(false);
            }

   
             
            var narrativeData = dialogue.NodeLinks.First(); //Entrypoint node

            ProceedToNarrative(narrativeData.TargetNodeGuid);
        }

        


        private void ProceedToNarrative(string narrativeDataGUID)
        {
            var text = dialogue.DialogueNodeData.Find(x => x.NodeGUID == narrativeDataGUID).DialogueText;
            var charapter = dialogue.DialogueNodeData.Find(x => x.NodeGUID == narrativeDataGUID).DialogueChar;
            var choices = dialogue.NodeLinks.Where(x => x.BaseNodeGuid == narrativeDataGUID);
            var id = dialogue.DialogueNodeData.Find(x => x.NodeGUID == narrativeDataGUID).DialogueID;

            if (dialogueChar != null)
                dialogueChar.text = ProcessProperties(charapter);

            DialogueNodeID = ProcessProperties(id);
            m_text = ProcessProperties(text);
            StartCoroutine(WritterEffect(m_text));
            

            var buttons = buttonContainer.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                Destroy(buttons[i].gameObject);
            }

            foreach (var choice in choices)
            {
                    var button = Instantiate(choicePrefab, buttonContainer);
                    button.GetComponentInChildren<TextMeshProUGUI>().text = ProcessProperties(choice.PortName);
                    button.onClick.AddListener(() => ProceedToNarrative(choice.TargetNodeGuid));
                    if (!buttonAllwaysActiving)
                    {
                        button.onClick.AddListener(() => buttonsContainer.SetActive(false));
                    }

            }
        }

        private string ProcessProperties(string text)
        {
            foreach (var exposedProperty in dialogue.ExposedProperties)
            {
                m_characterInterval = m_tempInterval;
                text = text.Replace($"[{exposedProperty.PropertyName}]", exposedProperty.PropertyValue);   
            }
            return text;
        }

    IEnumerator WritterEffect(string tempText)
    {
        // Эффект побуквенной записи текста в диагловое окно
        // In char effect to text writting in dialogue window 
        isWtritting = true;

        // Восстанавливает изначальную скорость печати текста
        m_characterInterval = m_tempInterval;
        dialogueText.text = "";
        foreach (char letter in tempText.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(m_characterInterval);
        }
        isWtritting = false;
        buttonsContainer.SetActive(true);

        
    }


    private void FixedUpdate()
    {
        //По клику ускоряет печать текста, для нетерпеливых, или если уже видели данный текст
        if (Input.GetMouseButton(0))
        {
            m_characterInterval = (float)0.001f;
        }

        // Старая проверка записи текста // Убрана по причине улучшения общего кода
        /*
            if (isWtritting)
            {
                buttonsContainer.SetActive(false);
            }
        */
       

        /*
         
        // Writter Effect for text
        // Эффект написания текста, для диалговов и внутриНодового текста.
        m_cumalativeDeltaTime += Time.deltaTime;
        while (m_cumalativeDeltaTime >= m_characterInterval && temperalText.Length < m_text.Length)
        {
            temperalText += m_text[temperalText.Length];
            m_cumalativeDeltaTime -= m_characterInterval;
            //dialogueText.text += temperalText;
        }
        dialogueText.text = temperalText;

        */
    }
}

