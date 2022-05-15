using UnityEngine;

public class DNA : MonoBehaviour
{
    //Genoma que contém os movimentos do mouse
    public int[] Genoma { get; set; }
    //Aptidão do DNA
    public float Fitness { get; set; }

    //Quantidade de quadrantes do desenho
    private int precisao = 16;

    public DNA(int tamanho)
    {
        //Cria um novo DNA
        Genoma = new int[tamanho];
        for (int i = 0; i < Genoma.Length; i++)
        {
            //Gera um número aleatório entre 0 e 15 para cada genoma do DNA
            Genoma[i] = Random.Range(0, precisao);
        }
    }

    //Converte os valores que estão no vetor 'Genoma' para uma única string
    //Exemplo: Vetor: 0,5,3 = String: "0-5-3"
    public string GetDNAString()
    {
        string _dna = "";
        for (int i = 0; i < Genoma.Length; i++)
        {
            if (i != Genoma.Length - 1)
                _dna += Genoma[i].ToString() + "-";
            else
                _dna += Genoma[i].ToString();
        }
        return _dna;
    }

    //Cálculo Fitness - Verifica quais genomas do DNA estão certos e atribui
    //um valor de aptidão de 0 a 1
    public void CalcFitness(string target)
    {
        int score = 0;
        //Separa a string target em valores de um vetor
        string[] targetGene = target.Split('-');
        for (int i = 0; i < Genoma.Length; i++)
        {
            //Verifica se o genoma criado é igual ao genoma desejado
            //Verifica cada valor do vetor Genoma, se for igual ao valor
            //correspondente do vetor 'targetGene' adiciona 1 ponto na variável score
            if (Genoma[i] == int.Parse(targetGene[i]))
                score++;
        }

        //Calcula a aptidão do Genoma
        Fitness = (float)score / (float)targetGene.Length;
    }

    //Cross-Over
    public DNA Cruzar(DNA parceiro)
    {
        // Novo indivíduo
        DNA novoIndividuo = new DNA(Genoma.Length);

        //Define o ponto de cruzamento
        int pontoCruzamento = Random.Range(0, Genoma.Length);

        // Metade de um, metade de outro (com base no ponto de cruzamento)
        for (int i = 0; i < Genoma.Length; i++)
        {
            if (i > pontoCruzamento)
                novoIndividuo.Genoma[i] = Genoma[i];
            else
                novoIndividuo.Genoma[i] = parceiro.Genoma[i];
        }
        return novoIndividuo;
    }

    //Para cada gene, se atender a taxa de mutação, troca o mesmo por outro 
    //aleatório
    public void Mutar(float taxaMutacao)
    {
        for (int i = 0; i < Genoma.Length; i++)
        {
            if (Random.value <= taxaMutacao)
            {
                Genoma[i] = Random.Range(0, precisao);
            }
        }
    }
}
