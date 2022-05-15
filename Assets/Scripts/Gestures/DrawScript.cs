using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JFortunato
{
    public class DrawScript : MonoBehaviour
    {
        public Shader shader;
        public SpriteRenderer canetaSprite;
        public Color color;
        public float limit = 0.3f;
        MouseGesture mouseGesture;
        GeneticAlgorithm_Novo geneticAlgorithm;
        Vector2 mousePos;
        public bool _canDraw = true;
        private bool podeDesenhar = false;
        public static bool isDrawing = false;

        List<Vector3> posList = new List<Vector3>();

        [SerializeField] private Transform caneta;

        public bool desenhando = false;

        LineRenderer line;

        private void Start()
        {
            mouseGesture = GetComponent<MouseGesture>();
            geneticAlgorithm = GetComponent<GeneticAlgorithm_Novo>();

            line = new GameObject().AddComponent<LineRenderer>();

            line.gameObject.tag = "Line";
            line.name = "Line";
            line.material = new Material(shader);
            line.material.color = color;
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            line.useWorldSpace = false;
            line.transform.position = new Vector3(line.transform.position.x, line.transform.position.y, -3);
        }

        void FixedUpdate()
        {
#if UNITY_STANDALONE_WIN
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward * 5;

            if (podeDesenhar)
            {
                if (_canDraw)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        RemoveLines();
                        mouseGesture.StartCapture();
                        posList.Add(mousePos);
                        StartCoroutine(Draw());
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                RemoveLines();
            }
#endif

#if UNITY_ANDROID
        touch = Input.GetTouch(0);
        mousePos = Camera.main.ScreenToWorldPoint(touch.position) + Vector3.forward * 5;
        if (canDraw)
        {
            posList.Add(mousePos);
            StartCoroutine(Draw());
        }
#endif
        }

        public void RemoveLines()
        {
            line.positionCount = 0;
            posList.Clear();
            mouseGesture.MoveString.Clear();
        }

        IEnumerator Draw()
        {
            line.transform.position = new Vector3(line.transform.position.x, line.transform.position.y, -3);

            while (Input.GetMouseButton(0))
            {
                isDrawing = true;
                float distance = Vector3.Distance(posList[posList.Count - 1], mousePos);
                if (distance >= limit)
                {
                    mouseGesture.CaptureHandler(mousePos);
                    mouseGesture.DrawPoints.Add(RoundVector(mousePos));
                    posList.Add(mousePos);
                    line.positionCount = posList.Count;
                    line.SetPositions(posList.ToArray());
                }
                yield return null;
            }
            isDrawing = false;
            PlayerPrefs.SetInt("_SizeDraw", mouseGesture.MoveString.Count);
            PlayerPrefs.SetString("_CurrentDraw", mouseGesture.ListToString());
            posList.Clear();
        }

        public Vector2 RoundVector(Vector2 vector)
        {
            decimal xd = Convert.ToDecimal(vector.x);
            decimal yd = Convert.ToDecimal(vector.y);
            float x = (float)Math.Round(xd, 1);
            float y = (float)Math.Round(yd, 1);

            vector = new Vector2(x, y);
            return vector;
        }

        public void DesenharIndividuo(List<Vector2> posicoes, float speed)
        {
            line.positionCount = 0;
            posList.Clear();
            StartCoroutine(AutomaticDraw(posicoes, speed));
        }

        private IEnumerator AutomaticDraw(List<Vector2> posicoes, float speed)
        {
            desenhando = true;
            for (int i = 0; i < posicoes.Count; i++)
            {
                caneta.position = posicoes[i];
                posList.Add(caneta.position);
                line.positionCount = posList.Count;
                line.SetPositions(posList.ToArray());
                yield return new WaitForSeconds(speed);
            }
            desenhando = false;
        }

        public void PodeDesenhar(bool b)
        {
            podeDesenhar = b;
        }
    }
}