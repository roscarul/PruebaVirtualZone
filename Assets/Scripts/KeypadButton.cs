using UnityEngine;

namespace LighthouseEscape
{
    // Tecla de un candado numérico:0-9 introduce el dígito, Clear borra y Confirm envía el código.
    public class KeypadButton : MonoBehaviour
    {
        public enum KeyAction
        {
            Digit,
            Clear,
            Confirm
        }

        [SerializeField] private KeyAction action = KeyAction.Digit;
        [SerializeField] private int digit = 0;

        private CombinationLock combinationLock;

        private void Awake()
        {
            combinationLock = GetComponentInParent<CombinationLock>();
        }

        public void Press()
        {
            if (combinationLock == null)
                return;

            if (action == KeyAction.Confirm)
                combinationLock.Confirm();
            else if (action == KeyAction.Clear)
                combinationLock.Clear();
            else
                combinationLock.Press(digit);
        }

        public string Prompt()
        {
            if (combinationLock == null)
                return "";

            if (combinationLock.Solved)
                return "Candado abierto";

            if (action == KeyAction.Confirm)
                return "E — confirmar";

            if (action == KeyAction.Clear)
                return "E — borrar";

            return "E — pulsar " + digit;
        }
    }
}
