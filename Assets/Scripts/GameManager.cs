using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace SA
{
    public class GameManager : MonoBehaviour
    {
        public int maxWidth = 15;
        public int maxHeight = 17;

        int _currentScore;
        int _highScore;

        public bool isGameOver;
        public bool isFirstInput;

        public Color color1;
        public Color color2;
        public Color playerColor;
        public Color appleColor = Color.red;

        public Transform cameraHolder;

        public Text currentScoreText;
        public Text highScoreText;

        GameObject tailParent;
        GameObject playerObj;
        GameObject appleObj;
        node PlayerNode;
        node prevPlayerNode;
        node appleNode;
        Sprite playerSprite;

        List<node> avaliableNode = new List<node>();
        List<SpecialNode> tail = new List<SpecialNode>();

        public float moveRate = 0.5f;
        float timer;

        bool up, left, right, down;
        Direction targetDiretion;
        Direction currentDirection;

        public enum Direction
        {
            up, down, left, right
        }
        node[,] grid;
        GameObject mapObject;
        SpriteRenderer mapRenderer;

        public UnityEvent onStart;
        public UnityEvent onGameOver;
        public UnityEvent firstInput;
        public UnityEvent onScore;
        // Start is called before the first frame update
        #region Init
        void Start()
        {
            onStart.Invoke();
        }

        public void StartNewGame()
        {
            ClearReferences();
            CreateMap();
            PlacePlayer();
            PLaceCamera();
            CreateApple();
            //  targetDiretion = Direction.right;
            isGameOver = false;
            _currentScore = 0;
            UpdateScore();
        }
        public void ClearReferences()
        {
            if (mapObject != null)
            {
                Destroy(mapObject);
            }
            if (playerObj != null)
            {
                Destroy(playerObj);
            }
            if (appleObj != null)
            {
                Destroy(appleObj);
            }
           
            foreach (var t in tail)
            {
                if (t.obj != null)
                {
                    Destroy(t.obj);
                }
            }
            tail.Clear();
            avaliableNode.Clear();
            grid = null;
        }

        void CreateApple()
        {
            appleObj = new GameObject("Apple");
            SpriteRenderer spriteRenderer = appleObj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSprite(appleColor);
            spriteRenderer.sortingOrder = 1;
            RandomPlaceApple();
        }
        void PLaceCamera()
        {
            node n = getNode(maxWidth / 2, maxHeight / 2);
            Vector3 p = n.worldPosition;
            p += Vector3.one * 0.5f;
            p.z = -5;
            cameraHolder.position = p;
        }

        public void CreateMap()
        {
            mapObject = new GameObject("Map");
            mapObject.transform.position = new Vector3(0, 0, 0);
            mapRenderer = mapObject.AddComponent<SpriteRenderer>();
            grid = new node[maxWidth, maxHeight];
            Texture2D txt = new Texture2D(maxWidth, maxHeight);
            for (int i = 0; i < maxWidth; i++)
            {
                for (int j = 0; j < maxHeight; j++)
                {
                    Vector3 tp = Vector3.zero;
                    tp.x = i;
                    tp.y = j;

                    node n = new node()
                    {
                        x = i,
                        y = j,
                        worldPosition = tp
                    };
                    grid[i, j] = n;
                    avaliableNode.Add(n);
                    #region Visual
                    if (i % 2 != 0)
                    {
                        if (j % 2 != 0)
                        {
                            txt.SetPixel(i, j, color1);
                        }
                        else
                        {
                            txt.SetPixel(i, j, color2);
                        }
                    }
                    else
                    {
                        if (j % 2 != 0)
                        {
                            txt.SetPixel(i, j, color2);
                        }
                        else
                        {
                            txt.SetPixel(i, j, color1);
                        }
                    }
                    #endregion
                }
            }
            txt.filterMode = FilterMode.Point;
            txt.Apply();
            Rect rect = new Rect(0, 0, maxWidth, maxHeight);
            Sprite sprite = Sprite.Create(txt, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            sprite.name = "Map";
            mapRenderer.sprite = sprite;
        }


        // Update is called once per frame
        void PlacePlayer()
        {
            playerObj = new GameObject("Player");
            SpriteRenderer playerRender = playerObj.AddComponent<SpriteRenderer>();
            playerSprite = CreateSprite(playerColor);

            playerRender.sprite = playerSprite;
            playerRender.sortingOrder = 1;
            PlayerNode = getNode(3, 3);
            PlacePlayerObjet(playerObj, PlayerNode.worldPosition);

            playerObj.transform.localScale = Vector3.one * 1.2f;
            tailParent = new GameObject("tailParent");
        }
        #endregion


        #region Update
        void GetInput()
        {
            up = Input.GetButtonDown("Up");
            down = Input.GetButtonDown("Down");
            left = Input.GetButtonDown("Left");
            right = Input.GetButtonDown("Right");
        }

        void setPLayerDirection()
        {
            if (up)
            {
                SeteDirection(Direction.up);

            }
            else if (down)
            {
                SeteDirection(Direction.down);

            }
            else if (left)
            {
                SeteDirection(Direction.left);

            }
            else if (right)
            {
                SeteDirection(Direction.right);

            }
        }
        void SeteDirection(Direction d)
        {
            if (!IsOpposite(d))
            {
                targetDiretion = d;

            }
        }

        void MovePlayer()
        {

            int x = 0;
            int y = 0;

            switch (currentDirection)
            {
                case Direction.up:
                    y = 1;
                    break;
                case Direction.down:
                    y = -1;
                    break;
                case Direction.left:
                    x = -1;
                    break;
                case Direction.right:
                    x = 1;
                    break;
            }
            node targetNode = getNode(PlayerNode.x + x, PlayerNode.y + y);
            if (targetNode == null)
            {
                //GameOver
                onGameOver.Invoke();
            }
            else
            {
                if (isTailNode(targetNode))
                {
                    //gameOver
                    onGameOver.Invoke();
                }
                else
                {
                    bool isScore = false;
                    if (targetNode == appleNode)
                    {
                        isScore = true;

                    }
                    node previousNode = PlayerNode;
                    avaliableNode.Add(previousNode);


                    if (isScore)
                    {
                        tail.Add(CreateTailNode(previousNode.x, previousNode.y));
                        avaliableNode.Remove(previousNode);
                    }
                    //move Tail
                    MoveTail();

                    PlacePlayerObjet(playerObj, targetNode.worldPosition);

                    PlayerNode = targetNode;
                    avaliableNode.Remove(PlayerNode);
                    if (isScore)
                    {
                        _currentScore++;
                        if(_currentScore >= _highScore)
                        {
                            _highScore = _currentScore;
                        }
                        onScore.Invoke();
                        if (avaliableNode.Count > 0)
                        {

                            RandomPlaceApple();
                        }
                        else
                        {
                            //you win
                        }
                    }
                }
            }
        }
        void MoveTail()
        {
            node prevNode = null;
            for (int i = 0; i < tail.Count; i++)
            {
                SpecialNode p = tail[i];
                avaliableNode.Add(p.Node);
                if (i == 0)
                {
                    prevNode = p.Node;
                    p.Node = PlayerNode;
                }
                else
                {
                    node prev = p.Node;
                    p.Node = prevNode;
                    prevNode = prev;
                }
                avaliableNode.Remove(p.Node);
                PlacePlayerObjet(p.obj, p.Node.worldPosition);

            }
        }
        private void Update()
        {
            if(isGameOver)
            {
                if(Input.GetKeyDown(KeyCode.R))
                {
                    onStart.Invoke();
                }
                return;
            }
            GetInput();
           
            if (isFirstInput)
            {
                setPLayerDirection();
                timer += Time.deltaTime;
                if (timer > moveRate)
                {
                    timer = 0;
                    currentDirection = targetDiretion;
                    MovePlayer();
                }
            }
            else
            {
                if(up || down || left || right)
                {
                    isFirstInput = true;
                    firstInput.Invoke();
                }
            }
        }

        #endregion
        #region Utilities
        public void GameOver()
        {
            isGameOver = true;
            isFirstInput = false;
        }

        public void UpdateScore()
        {
            currentScoreText.text = _currentScore.ToString();
            highScoreText.text = _highScore.ToString();
        }
        void PlacePlayerObjet(GameObject obj, Vector3 pos)
        {
            pos += Vector3.one * 0.5f;
            obj.transform.position = pos;
        }
        void RandomPlaceApple()
        {
            int ran = Random.Range(0, avaliableNode.Count);
            node n = avaliableNode[ran];
            PlacePlayerObjet(appleObj, n.worldPosition);

            appleNode = n;
        }

        bool IsOpposite(Direction d)
        {
            switch (d)
            {
                default:
                case Direction.up:
                    if (currentDirection == Direction.down)
                        return true;
                    else
                        return false;
                case Direction.down:
                    if (currentDirection == Direction.up)
                        return true;
                    else
                        return false;
                case Direction.left:
                    if (currentDirection == Direction.right)
                        return true;
                    else
                        return false;
                case Direction.right:
                    if (currentDirection == Direction.left)
                        return true;
                    else
                        return false;
            }

        }

        bool isTailNode(node n)
        {
            for (int i = 0; i < tail.Count; i++)
            {
                if (tail[i].Node == n)
                {
                    return true;
                }

            }
            return false;
        }
        node getNode(int x, int y)
        {
            if (x < 0 || x > maxWidth - 1 || y < 0 || y > maxHeight - 1)
            {
                return null;
            }
            return grid[x, y];
        }
        SpecialNode CreateTailNode(int x, int y)
        {
            SpecialNode s = new SpecialNode();
            s.Node = getNode(x, y);
            s.obj = new GameObject();
            s.obj.transform.parent = tailParent.transform;
            s.obj.transform.position = s.Node.worldPosition;
            SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();
            s.obj.transform.localScale = Vector3.one * 0.95f;
            r.sprite = playerSprite;
            r.sortingOrder = 1;
            return s;
        }
        Sprite CreateSprite(Color targetColor)
        {
            Texture2D txt = new Texture2D(1, 1);
            txt.SetPixel(0, 0, targetColor);
            txt.Apply();
            txt.filterMode = FilterMode.Point;
            Rect rect = new Rect(0, 0, 1, 1);
            return Sprite.Create(txt, rect, Vector2.one * 0.5f, 1, 0, SpriteMeshType.FullRect);
        }
        #endregion
    }



}