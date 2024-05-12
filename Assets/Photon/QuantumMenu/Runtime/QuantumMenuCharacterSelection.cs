
using UnityEngine;
using UnityEngine.UI;

namespace Quantum.Menu
{
    public class QuantumMenuCharacterSelection : QuantumMenuUIScreen
    {
        [SerializeField] private CharacterInfo[] characters;
        [SerializeField] private UI_CharacterInfo characterInfoDisplay;
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private Transform characterStatsParent;
        private bool _alreadyShown;
        private QuantumMenuConnectArgs _connectionArgs;
        
        public virtual void OnBackButtonPressed() {
            Controller.Show<QuantumMenuUIMain>();
        }

        public override void Awake()
        {
            _connectionArgs = ConnectionArgs as QuantumMenuConnectArgs;
        }

        public override void Show()
        {
            base.Show();
            if(_alreadyShown) return;
            _alreadyShown = true;
            
            foreach (var character in characters)
            {
                var display = Instantiate(characterInfoDisplay, characterStatsParent);
                display.Init(character, CharacterSelected, toggleGroup, _connectionArgs.RuntimePlayers[0].PlayerAvatar == character.EntityPrototype);
            }
            
        }

        private void CharacterSelected(EntityPrototype obj)
        {
            _connectionArgs.RuntimePlayers[0].PlayerAvatar = obj;
        }
    }
}