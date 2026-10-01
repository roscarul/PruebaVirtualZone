using System.Collections.Generic;
using UnityEngine;

namespace LighthouseEscape
{
    // Candado numérico de teclado: se introduce el código dígito a dígito y al completarlo se
    // comprueba (acierto → log + caja verde + targets; fallo → se borra el intento).
    // Los destinos al desbloquear se enchufan en unlockTargets (vacío = solo log).
    public class CombinationLock : MonoBehaviour
    {
        [SerializeField] private int[] targetCode = { 3, 1, 4 };
        [SerializeField] private MonoBehaviour[] unlockTargets;

        private readonly List<int> entry = new List<int>();
        private Renderer boxRenderer;

        public bool Solved { get; private set; }

        private int TargetLength => targetCode != null && targetCode.Length > 0 ? targetCode.Length : 3;

        public string CodeText
        {
            get
            {
                if (Solved)
                    return "Candado abierto";

                string text = "";
                for (int i = 0; i < TargetLength; i++)
                {
                    if (i > 0)
                        text += "-";
                    text += i < entry.Count ? entry[i].ToString() : "_";
                }

                return "Código: " + text;
            }
        }

        private void Awake()
        {
            boxRenderer = GetComponent<Renderer>();
        }

        // digit 0-9 lo introduce; el código se envía con el botón Confirmar.
        public void Press(int digit)
        {
            if (Solved || digit < 0)
                return;

            entry.Add(digit);
        }

        public void Clear()
        {
            if (!Solved)
                entry.Clear();
        }

        public void Confirm()
        {
            if (Solved || entry.Count == 0)
                return;

            Check();
        }

        private void Check()
        {
            bool correct = targetCode != null && entry.Count == targetCode.Length;

            for (int i = 0; correct && i < targetCode.Length; i++)
            {
                if (entry[i] != targetCode[i])
                    correct = false;
            }

            entry.Clear();

            if (correct)
                Solve();
            else
                Debug.Log("[Puzzle] Código incorrecto");
        }

        private void Solve()
        {
            Solved = true;

            Debug.Log("[Puzzle] Candado numérico abierto: " + Join(targetCode));

            if (boxRenderer != null)
                boxRenderer.material.color = new Color(0.45f, 1f, 0.5f);

            if (unlockTargets == null)
                return;

            foreach (MonoBehaviour target in unlockTargets)
            {
                if (target is IUnlockable unlockable)
                    unlockable.Unlock();
            }
        }

        private string Join(int[] values)
        {
            if (values == null)
                return "";

            string text = "";
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0)
                    text += "-";
                text += values[i];
            }

            return text;
        }
    }
}
