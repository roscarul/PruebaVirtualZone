namespace LighthouseEscape
{
    // Contrato "algo se desbloquea": lo implementan la puerta, tapas, candados, etc.
    // Las fuentes de puzzle (placa, candado, secuencia) llaman a esto desde sus targets.
    public interface IUnlockable
    {
        void Unlock();
        void Lock();
    }
}
