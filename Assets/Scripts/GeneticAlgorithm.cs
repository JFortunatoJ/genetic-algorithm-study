using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JFortunato
{
    public class GeneticAlgorithm : MonoBehaviour
    {
        [SerializeField] private MouseGesture mouseGesture;
        [SerializeField] private DrawScript drawScript;
        [SerializeField] private Text txtGeracao;

        public struct Individuo
        {
            public string DNA { get; set; }
            public List<Vector2> Pontos { get; set; }
            public int Aptidao { get; set; }
        }

        private List<Individuo> listaIndividuos = new List<Individuo>();

        #region Variaveis
        [Space]
        public int qtdIndividuos = 10;
        [Space]
        [Range(0, 1)]
        public float taxaCruzamento = 0.8f;
        [Range(0, 1)]
        public float taxaMutacao = 0.1f;
        [Space]
        public int taxaDeCorte = 10;
        [Space]
        public int geracao = 0;
        [Space]
        public float velocidade = 0.5f;
        [Range(0, 10)]
        public float mediaParada = 2f;
        [Space]
        public float comprimentoSegmento = 0.3f;
        public float anguloMax = 6.3f;
        #endregion

        float tamanhoLinha;    //Distância entre um ponto e outro.
        int precisao = 36;
        int qtdMovimentos;
        string drawString;
        [HideInInspector]
        public bool continuar = true;

        private void Start()
        {
            tamanhoLinha = drawScript.limit;
            precisao = mouseGesture.DEFAULT_NB_SECTORS;
            drawString = PlayerPrefs.GetString("_CurrentDraw");
            qtdMovimentos = PlayerPrefs.GetInt("_SizeDraw");
        }

        public void Iniciar()
        {
            StartCoroutine(Inicio());
        }

        IEnumerator Inicio()
        {
            GerarPopulacaoInicial();
            AvaliarPopulacao();
            do
            {
                Selecionar();
                Cruzamento();
                Mutacao();
                AvaliarPopulacao();
                yield return null;
                AtualizaGeracao();
            } while (!Parar());
            print("Codigo esperado: " + drawString + " / Melhor código: " + MelhorIndividuo().DNA +
                "\nGerações: " + geracao);
            print("Saiu");
            ConvertDNA(MelhorIndividuo().DNA);
        }

        private void ConvertDNA(string dna)
        {
            string[] cromossomo = dna.Split('-');

            Individuo individuo = new Individuo();
            List<Vector2> pontos = new List<Vector2>();
            pontos.Add(Vector2.zero);

            GameObject p = new GameObject("Helper");
            GameObject f = new GameObject("Child");
            f.transform.parent = p.transform;
            p.transform.position = pontos[0];

            for (int i = 0; i < cromossomo.Length; i++)
            {
                int d = int.Parse(cromossomo[i]);

                float a = (d == 0) ? (anguloMax * Mathf.Deg2Rad) : ((anguloMax / 32) * d) * Mathf.Deg2Rad;
                float x = comprimentoSegmento * Mathf.Cos(a);
                float y = comprimentoSegmento * Mathf.Sin(a);

                f.transform.localPosition = new Vector2(x, y);
                pontos.Add(f.transform.position);
                p.transform.position = f.transform.position;
            }

            Destroy(p);

            individuo.Pontos = pontos;

            drawScript.DesenharIndividuo(individuo.Pontos, 0.5f);
        }

        public void GerarPopulacaoInicial()
        {
            for (int i = 0; i < qtdIndividuos; i++)
            {
                listaIndividuos.Add(GerarIndividuo());
            }
        }

        private Individuo GerarIndividuo()
        {
            Individuo individuo = new Individuo();
            individuo.Pontos = new List<Vector2>();
            string _dna = null;
            for (int j = 0; j < qtdMovimentos; j++)
            {
                int _cromossomo = UnityEngine.Random.Range(0, precisao);
                if (j != qtdMovimentos - 1)
                    _dna += _cromossomo.ToString() + "-";
                else
                    _dna += _cromossomo.ToString();
            }

            int aptidao = mouseGesture.LevenshteinDistance(drawString, _dna);

            individuo.DNA = _dna;
            individuo.Aptidao = aptidao;
            return individuo;
        }

        public void AvaliarPopulacao()
        {
            List<Individuo> tempList = new List<Individuo>();

            //Determinando o melhor inidivíduo
            int menorDist = mouseGesture.LevenshteinDistance(drawString, listaIndividuos[0].DNA);
            for (int i = 0; i < qtdIndividuos; i++)
            {
                int x = mouseGesture.LevenshteinDistance(drawString, listaIndividuos[i].DNA);

                Individuo ind = new Individuo { Aptidao = x, DNA = listaIndividuos[i].DNA, Pontos = listaIndividuos[i].Pontos };
                tempList.Add(ind);
            }

            //Atualizando a taxa de corte
            float m = MediaAptidoes(tempList);
            taxaDeCorte = (int)(m - (m * 0.1f));

            //Removendo os menos aptos

            tempList.RemoveAll(item => item.Aptidao > taxaDeCorte);

            while (tempList.Count < qtdIndividuos)
            {
                tempList.Add(GerarIndividuo());
            }

            listaIndividuos = tempList;
        }

        //Roleta e Elitismo
        private void Selecionar()
        {
            //Somatória das aptidões
            float soma = 0;
            for (int i = 0; i < qtdIndividuos; i++)
            {
                soma += listaIndividuos[i].Aptidao;
            }

            //Calculando a porcentagem de cada aptidão
            float[] proporcao = new float[qtdIndividuos];
            for (int i = 0; i < qtdIndividuos; i++)
            {
                proporcao[i] = (float)Math.Round((100f / soma) * listaIndividuos[i].Aptidao, 2);
            }

            //Calculando as somas das porcentagens
            for (int i = 0; i < qtdIndividuos; i++)
            {
                if (i != 0)
                    proporcao[i] = proporcao[i] + proporcao[i - 1];
            }

            //Calculando a proporção
            for (int i = 0; i < qtdIndividuos; i++)
            {
                proporcao[i] = (float)Math.Round(proporcao[i] / 100, 2);
            }

            //Selecionando os mais aptos
            //Não tenho certeza se está 100% certo
            List<Individuo> novaListaIndividuos = new List<Individuo>();

            //Elitismo: Adiciona sempre o melhor indivíduo na próxima população
            novaListaIndividuos.Add(MelhorIndividuo());
            do
            {
                float numeroAleatorio = UnityEngine.Random.value;
                int index = 0;
                for (int i = 0; i < qtdIndividuos; i++)
                {
                    if (proporcao[i] >= numeroAleatorio)
                    {
                        index = i;
                        break;
                    }
                }
                novaListaIndividuos.Add(listaIndividuos[index]);
            } while (novaListaIndividuos.Count < qtdIndividuos);
            //print("Selecionou");
            listaIndividuos = novaListaIndividuos;
        }

        private void Cruzamento()
        {
            List<Individuo> novosIndividuos = new List<Individuo>();
            for (int i = 0; i < qtdIndividuos; i += 2)
            {
                //Talvez tenha que mudar o ponto de cruzamento pra ser entre 1 e qtdMovimentos
                int pontoCruzamento = UnityEngine.Random.Range(0, qtdMovimentos - 1);
                float probabilidade = UnityEngine.Random.value;

                //Pode ocorrer cruzamento
                if (probabilidade <= taxaCruzamento)
                {
                    string[] _dna1 = listaIndividuos[i].DNA.Split('-');
                    string[] _dna2 = listaIndividuos[i + 1].DNA.Split('-');

                    string[] _novoDna1 = new string[_dna1.Length];
                    string[] _novoDna2 = new string[_dna2.Length];

                    #region Cruzamento_1
                    //Cross-over
                    /*for (int j = 0; j < pontoCruzamento + 1; j++)
                    {
                        _novoDna1[j] = _dna1[j];
                    }
                    for (int k = pontoCruzamento; k < qtdMovimentos; k++)
                    {
                        _novoDna1[k] = _dna2[k];
                    }

                    for (int j = 0; j < pontoCruzamento + 1; j++)
                    {
                        _novoDna2[j] = _dna2[j];
                    }
                    for (int k = pontoCruzamento; k < qtdMovimentos; k++)
                    {
                        _novoDna2[k] = _dna1[k];
                    }*/
                    #endregion

                    #region Cruzamento_2
                    for (int j = 0; j < _dna1.Length; j++)
                    {
                        int chance = UnityEngine.Random.Range(0, 2);
                        if (chance == 0)
                        {
                            _novoDna1[j] = _dna1[j];
                        }
                        else
                        {
                            _novoDna1[j] = _dna2[j];
                        }
                    }

                    for (int j = 0; j < _dna2.Length; j++)
                    {
                        int chance = UnityEngine.Random.Range(0, 2);
                        if (chance == 0)
                        {
                            _novoDna2[j] = _dna1[j];
                        }
                        else
                        {
                            _novoDna2[j] = _dna2[j];
                        }
                    }

                    #endregion
                    //Contruindo o dna
                    string DNA1 = null;
                    string DNA2 = null;
                    for (int j = 0; j < _novoDna1.Length; j++)
                    {
                        if (j != _novoDna1.Length - 1)
                        {
                            DNA1 += _novoDna1[j] + "-";
                            DNA2 += _novoDna2[j] + "-";
                        }
                        else
                        {
                            DNA1 += _novoDna1[j];
                            DNA2 += _novoDna2[j];
                        }
                    }

                    Individuo ind1 = new Individuo
                    {
                        Aptidao = 100,
                        DNA = DNA1,
                        Pontos = new List<Vector2>()
                    };

                    Individuo ind2 = new Individuo
                    {
                        Aptidao = 100,
                        DNA = DNA2,
                        Pontos = new List<Vector2>()
                    };

                    novosIndividuos.Add(ind1);
                    novosIndividuos.Add(ind2);

                    //print("Cruzou: Individuo " + i + " Individuo " + (i + 1));
                }
                //Se não ocorrer o cruzamento, mantém os dois individuos na nova população
                else
                {
                    novosIndividuos.Add(listaIndividuos[i]);
                    novosIndividuos.Add(listaIndividuos[i + 1]);
                    //print("Não Cruzou");
                }
            }

            listaIndividuos = novosIndividuos;
        }

        //Sem usar binários
        private void Mutacao()
        {
            List<Individuo> tempList = new List<Individuo>();
            Individuo ind;

            for (int i = 0; i < qtdIndividuos; i++)
            {
                int pontoMutacao = UnityEngine.Random.Range(0, qtdMovimentos);
                float probabilidade = UnityEngine.Random.value;

                if (probabilidade <= taxaMutacao)
                {
                    string[] _dna = listaIndividuos[i].DNA.Split('-');
                    int novoGene = UnityEngine.Random.Range(0, precisao);
                    _dna[pontoMutacao] = novoGene.ToString();
                    string novoDNA = null;

                    for (int j = 0; j < _dna.Length; j++)
                    {
                        if (j != _dna.Length - 1)
                        {
                            novoDNA += _dna[j] + "-";
                        }
                        else
                        {
                            novoDNA += _dna[j];
                        }
                    }

                    ind = new Individuo
                    {
                        Aptidao = listaIndividuos[i].Aptidao,
                        DNA = novoDNA,
                        Pontos = listaIndividuos[i].Pontos
                    };

                    //print("Mutou");
                }
                else
                {
                    ind = new Individuo
                    {
                        Aptidao = listaIndividuos[i].Aptidao,
                        DNA = listaIndividuos[i].DNA,
                        Pontos = listaIndividuos[i].Pontos
                    };
                }
                tempList.Add(ind);
            }
            listaIndividuos = tempList;
        }

        public Individuo MelhorIndividuo()
        {
            int melhor = int.MaxValue;
            int index = 0;
            for (int i = 0; i < qtdIndividuos; i++)
            {
                if (listaIndividuos[i].Aptidao < melhor)
                {
                    melhor = listaIndividuos[i].Aptidao;
                    index = i;
                }
            }

            return listaIndividuos[index];
        }

        private float MediaAptidoes(List<Individuo> _listaIndividuos)
        {
            float soma = 0;
            float media;
            for (int i = 0; i < qtdIndividuos; i++)
            {
                soma += _listaIndividuos[i].Aptidao;
            }
            media = soma / qtdIndividuos;
            return media;
        }

        private bool Parar()
        {
            for (int i = 0; i < listaIndividuos.Count; i++)
            {
                if (listaIndividuos[i].Aptidao <= mediaParada)
                    return true;
            }
            return false;
        }

        private void AtualizaGeracao()
        {
            geracao++;
            txtGeracao.text = "Geração: " + geracao.ToString();
        }

        public void MostrarIndividuos()
        {
            foreach (Individuo i in listaIndividuos)
            {
                print(i.DNA + ": " + i.Aptidao);
                for (int j = 0; j < i.Pontos.Count; j++)
                {
                    print(i.Pontos[j]);
                }
            }
        }
    }
}