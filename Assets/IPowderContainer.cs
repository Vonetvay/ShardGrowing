using System.Collections.Generic;

public interface IPowderContainer
{
    // Принимает целую структуру смеси и возвращает количество, которое физически удалось поместить
    float ReceivePowder(PowderComposition mix);
    float GetAvailableSpace();

    bool CanIGetMixType(PowderComposition mix);
}

public interface IPowderContainerDynamic : IPowderContainer 
{
    PowderComposition GivePowderPortion(float amount);

    float GetTotalAmount();

    PowderComposition GetComposition();
}
