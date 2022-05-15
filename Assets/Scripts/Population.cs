using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Population : MonoBehaviour
{
    float taxaMutacao;                      // Taxa de Mutação
    float taxaCruzamento;
    DNA[] populacao;                        // Vetor com todos os indivíduos dessa população
    List<DNA> MatingPool = new List<DNA>(); // Vetor que será usado como 'mating pool'
    string targetString;                    // Genoma desejado
    int geracoes;                           // Número de gerações
    bool terminou;                          // true quando terminar de evoluir
    int pontuacaoPerfeita;

    //Método Construtor pra criar a população
    public Population(string _targetString, float _taxaMutacao, float _taxaCruzamento, int _tamanho)
    {
        //Define as propriedades da população com base nos parâmetros
        targetString = _targetString;
        taxaMutacao = _taxaMutacao;
        taxaCruzamento = _taxaCruzamento;
        populacao = new DNA[_tamanho]; //Inicializa o vetor com que terá os indivíduos

        //Pega a quantidade de movimentos do desenho feito
        int tamDNA = PlayerPrefs.GetInt("_SizeDraw");

        //Inicializa cada indivíduo da população
        for (int i = 0; i < populacao.Length; i++)
        {
            populacao[i] = new DNA(tamDNA);
        }

        //Calcula a aptidão da população gerada
        CalcFitness();
        //Inicializa a Lista
        MatingPool.Clear();
        terminou = false;
        geracoes = 0;

        //Quando um dos indivíduos tiver a aptidão igual a pontuacaoPerfeita,
        //significa que o objetivo foi atingido e o programa pode parar
        pontuacaoPerfeita = 1;
    }

    // Calcula a aptidão de cada indivíduo da população
    public void CalcFitness()
    {
        for (int i = 0; i < populacao.Length; i++)
        {
            populacao[i].CalcFitness(targetString);
        }
    }

    public void Selecionar()
    {
        //Limpa a Lista
        MatingPool.Clear();

        //Pega a aptidão do melhor DNA (indivíduo)
        float maxFitness = 0;
        for (int i = 0; i < populacao.Length; i++)
        {
            if (populacao[i].Fitness > maxFitness)
            {
                maxFitness = populacao[i].Fitness;
            }
        }

        //Baseado na aptidão, cada indivíduo será adicionado na lista MatingPool
        //um certo número de vezes.

        //Com aptidão alta, será adicionado mais vezes na lista, tendo mais chance de ser
        //escolhido para cruzar.

        //Com aptidão baixa, menos vezes na lista, menos chances de ser escolhido

        for (int i = 0; i < populacao.Length; i++)
        {
            //'Converte' a aptidão de cada indivíduo para um valor entre 0 e 1
            float fitness = Map(populacao[i].Fitness, 0, maxFitness, 0, 1);
            //Quantas vezes será adicionado na lista
            int n = (int)(fitness * 100);
            for (int j = 0; j < n; j++)
            {
                //Adiciona o indivíduo 'n vezes' na lista
                MatingPool.Add(populacao[i]);
            }
        }
    }

    //'Escala' um valor entre os limites que são definidos nos parâmetros
    private float Map(float value, float from, float to, float from2, float to2)
    {
        if (value <= from2)
        {
            return from;
        }
        else if (value >= to2)
        {
            return to;
        }
        else
        {
            return (to - from) * ((value - from2) / (to2 - from2)) + from;
        }
    }

    //Cria uma nova geração
    public void GerarPopulacao()
    {
        // Repõe a população com os 'filhos' gerados dos pais escolhidos da 'MatingPool'
        for (int i = 0; i < populacao.Length; i++)
        {
            //Escolhe indices aleatórios para escolher os pais
            int a = Random.Range(0, MatingPool.Count);
            int b = Random.Range(0, MatingPool.Count);
            //Escolhe os pais com os índices
            DNA pai_1 = MatingPool[a];
            DNA pai_2 = MatingPool[b];

            float x = Random.value;
            //Verifica se pode cruzar
            if (x < taxaCruzamento)
            {
                //Gera o filho com base no cruzamento dos pais
                DNA filho = pai_1.Cruzar(pai_2);
                //Faz mutação no filho
                filho.Mutar(taxaMutacao);
                //Adiciona o filho na população
                populacao[i] = filho;
            }
            else //Senão, mantém o pai
            {
                populacao[i] = pai_1;
            }
        }
        geracoes++;
    }

    //Verifica o melhor indivíduo da população (maior aptidão)
    public string MelhorDNA()
    {
        float melhor = 0.0f;
        int indexMelhor = 0;
        for (int i = 0; i < populacao.Length; i++)
        {
            if (populacao[i].Fitness > melhor)
            {
                indexMelhor = i;
                melhor = populacao[i].Fitness;
            }
        }

        if (melhor == pontuacaoPerfeita)
            terminou = true;

        //Retorna o DNA do melhor indivíduo
        return populacao[indexMelhor].GetDNAString();
    }

    //Calcula a média de aptidão da população
    public float GetMediaAptidao()
    {
        float total = 0;
        for (int i = 0; i < populacao.Length; i++)
        {
            total += populacao[i].Fitness;
        }
        return total / (populacao.Length);
    }

    //Retorna a varíavel pra verificar se já terminou
    public bool Terminou()
    {
        return terminou;
    }

    //Retorna a quantidade de gerações
    public int GetGeracoes()
    {
        return geracoes;
    }
}
