using UnityEngine;
using UnityEngine.UI;

namespace JFortunato
{
    public class UI_Manager : MonoBehaviour
    {
        [SerializeField] GeneticAlgorithm_Novo geneticAlgorithm;

        [SerializeField] Slider sldTaxaCruzamento;
        [SerializeField] Text txtTaxaCruzamento;

        [SerializeField] Slider sldTaxaMutacao;
        [SerializeField] Text txtTaxaMutacao;

        [SerializeField] Toggle tglDesenhar;

        [SerializeField] private Text txtGeracao;
        [SerializeField] private Text txtMediaFitness;
        [SerializeField] private InputField QtdIndividuos;

        private void Start()
        {
            ResetaCampos();
        }

        public void AtualizaInfo()
        {
            txtGeracao.text = "Geração: " + geneticAlgorithm.qtdGeracao;
            txtMediaFitness.text = "Média Fitness: " + geneticAlgorithm.mediaFitness;
            geneticAlgorithm.qtdIndividuos = int.Parse(QtdIndividuos.text);
        }

        public void AtualizaTaxaCruzamento()
        {
            geneticAlgorithm.taxaCruzamento = (float)System.Math.Round(sldTaxaCruzamento.value, 2);
            txtTaxaCruzamento.text = (geneticAlgorithm.taxaCruzamento * 100) + "%";
        }

        public void AtualizaTaxaMutacao()
        {
            geneticAlgorithm.taxaMutacao = (float)System.Math.Round(sldTaxaMutacao.value, 2);
            txtTaxaMutacao.text = (geneticAlgorithm.taxaMutacao * 100) + "%";
        }

        public void Desenhar()
        {
            geneticAlgorithm.drawScript._canDraw = tglDesenhar.isOn;
        }

        public void ResetaCampos()
        {
            sldTaxaCruzamento.value = geneticAlgorithm.taxaCruzamento;
            txtTaxaCruzamento.text = (geneticAlgorithm.taxaCruzamento * 100) + "%";

            sldTaxaMutacao.value = geneticAlgorithm.taxaMutacao;
            txtTaxaMutacao.text = (geneticAlgorithm.taxaMutacao * 100) + "%";

            geneticAlgorithm.qtdIndividuos = int.Parse(QtdIndividuos.text);

            tglDesenhar.isOn = false;
            geneticAlgorithm.drawScript._canDraw = tglDesenhar.isOn;

            geneticAlgorithm.qtdIndividuos = int.Parse(QtdIndividuos.text);
        }
    }
}