using JFortunato;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm_Novo : MonoBehaviour
{
    public DrawScript drawScript;
    [SerializeField] private UI_Manager uiManager;

    //Genoma desejado
    string target;
    Population population;

    #region Variaveis
    [Space]
    public int qtdIndividuos = 10;
    public int qtdGeracao = 0;
    public float mediaFitness = 0;
    [Space]
    [Range(0, 1)]
    public float taxaCruzamento = 0.8f;
    [Range(0, 1)]
    public float taxaMutacao = 0.1f;
    [Space]
    public float velocidade = 0.5f;
    [Space]
    public float comprimentoSegmento = 0.3f;
    public float anguloMax = 360f;
    #endregion

    public void Iniciar()
    {
        uiManager.ResetaCampos();

        target = PlayerPrefs.GetString("_CurrentDraw");

        //Cria uma nova população com o genoma desejado, taxa de mutação e qtd de indivíduos
        population = new Population(target, taxaMutacao, taxaCruzamento, qtdIndividuos);

        //Atualiza informações da tela
        AtualizaInfo();

        drawScript.RemoveLines();

        //Incia o Algorítmo
        StartCoroutine(Inicio());
    }

    IEnumerator Inicio()
    {
        do
        {
            //Gera a lista para escolher os parceiros (Mating Pool)
            population.Selecionar();
            //Cria a próxima geração
            population.GerarPopulacao();
            //Calcula a aptidão dos indivíduos
            population.CalcFitness();
            //'Printa' o DNA do melhor indivíduo de cada geração
            print(population.MelhorDNA());
            //Atualiza as informações da tela
            AtualizaInfo();
            ConvertDNA(population.MelhorDNA());
            do { yield return null; } while (drawScript.desenhando);
        } while (!population.Terminou()); //Repete enquanto não existir um indivíduo com DNA igual ao desejado

        //Converte o DNA do melhor indivíduo em desenho na tela
        ConvertDNA(population.MelhorDNA());
        drawScript.canetaSprite.gameObject.SetActive(false);
    }

    private void DesenhaMelhor(string DNA)
    {
        StartCoroutine(Desenha(DNA));
    }

    IEnumerator Desenha(string DNA)
    {
        ConvertDNA(population.MelhorDNA());
        do { yield return null; } while (drawScript.desenhando);
    }

    private void ConvertDNA(string dna)
    {
        string[] cromossomo = dna.Split('-');

        List<Vector2> pontos = new List<Vector2>();
        pontos.Add(Vector2.zero);

        GameObject p = new GameObject("Helper");
        GameObject f = new GameObject("Child");
        f.transform.parent = p.transform;
        p.transform.position = pontos[0];

        //Desenha usando coordenadas polares
        for (int i = 0; i < cromossomo.Length; i++)
        {
            int d = int.Parse(cromossomo[i]);

            float a = (d == 0) ? (anguloMax * Mathf.Deg2Rad) : ((anguloMax / 16) * d) * Mathf.Deg2Rad;
            float x = comprimentoSegmento * Mathf.Cos(a);
            float y = comprimentoSegmento * Mathf.Sin(a);

            f.transform.localPosition = new Vector2(x, y);
            pontos.Add(f.transform.position);
            p.transform.position = f.transform.position;
        }

        Destroy(p);

        drawScript.DesenharIndividuo(pontos, velocidade);
    }

    private void AtualizaInfo()
    {
        qtdGeracao = population.GetGeracoes();
        mediaFitness = (float)Math.Round(population.GetMediaAptidao(), 2);
        uiManager.AtualizaInfo();
    }
}
