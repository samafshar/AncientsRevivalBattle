public interface ICharacterVisitor
{
    void OnCharacterClicked(Character character);
    void OnCharacterSpellSelected(Character character, int index);    
}
