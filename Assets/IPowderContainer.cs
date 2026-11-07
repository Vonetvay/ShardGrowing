using System.Collections.Generic;

public interface IPowderContainer
{
    // Принимает целую структуру смеси и возвращает количество, которое физически удалось поместить
    float ReceivePowder(PowderComposition mix);
    float GetTotalAmount();
    float GetAvailableSpace();

    PowderComposition GivePowderPortion(float amount);

    PowderComposition GetComposition();
}
