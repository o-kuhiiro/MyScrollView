using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MyScrollView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 横を基準とする画面比
    const float WIDTH = 1920.0f;
    const float HEIGHT = 1080.0f;
    const float ASPECT = WIDTH / HEIGHT;

    // セルの数
    const int CELL_COUNT = 5;

    // 初期時の中央セルのインデックス
    const int INIT_CELL_INDEX = 2;

    // 加速度
    const float SENSITIVITY = 1.0f;

    // 余白を含めた1つのセル幅サイズ
    // 左余白(4px) + セル(376px) + 右余白(4px)
    const float CELL_SIZE = 384.0f;

    [SerializeField]
    GameObject baseCell;

    RectTransform _rectTransform;

    // Xの累計スクロール量
    float scrollPosition;

    // セルのTransform参照
    Dictionary<int, RectTransform> cellRectTransform = new Dictionary<int, RectTransform>();

    Vector2 onBeginDragPosition;
    float onBeginStartScrollPosition;
    Vector2 onDragPosition;

    [SerializeField]
    Camera outSideCamera;
    Transform _outSideCameraTransform;
    Vector3 leftOutSideCameraPosition;
    Vector3 rightOutSideCameraPosition;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        _outSideCameraTransform = outSideCamera.transform;

        float scale = (float)Screen.width / (float)Screen.height / ASPECT;
        leftOutSideCameraPosition = new Vector3(-WIDTH * scale, 0, 0);
        rightOutSideCameraPosition = new Vector3(WIDTH * scale, 0, 0);

        for (int i = 0; i < CELL_COUNT; i++)
        {
            var cell = Instantiate(baseCell, transform, false);

            var cellScript = cell.GetComponent<MyScrollViewCell>();
            cellScript.number.text = i.ToString();

            cellRectTransform.Add(i, cell.GetComponent<RectTransform>());
        }

        PositionUpdate();

        baseCell.SetActive(false);
    }

    // セルの位置を更新する
    // 中央セル→左側のセル→右側のセルの順に更新を行う
    void PositionUpdate()
    {
        if (scrollPosition < 0)
        {
            if (_outSideCameraTransform.localPosition != leftOutSideCameraPosition)
                _outSideCameraTransform.localPosition = leftOutSideCameraPosition;
        }
        else
        {
            if (_outSideCameraTransform.localPosition != rightOutSideCameraPosition)
                _outSideCameraTransform.localPosition = rightOutSideCameraPosition;
        }

        // 中央のセルのインデックス
        // ここの中央とは画面の中央ではなく、セルの中の中央という意味
        int centerIndex = GetLoopIndex(INIT_CELL_INDEX - (int)(scrollPosition / CELL_SIZE));
        // 中央のセルのX位置
        float centerPosition = scrollPosition % CELL_SIZE;

        // 中央のセル位置をセット
        cellRectTransform[centerIndex].localPosition = new Vector2(centerPosition, 0);

        // 左側のセル位置をセット
        float beforePosition = centerPosition;
        for(int i = 1; i <= CELL_COUNT / 2; i++)
        {
            int index = GetLoopIndex(centerIndex - i);
            float setPosition = beforePosition - CELL_SIZE;

            cellRectTransform[index].localPosition = new Vector2(setPosition, 0);

            beforePosition = setPosition;
        }

        // 右側のセル位置をセット
        beforePosition = centerPosition;
        for (int i = 1; i <= CELL_COUNT / 2; i++)
        {
            int index = GetLoopIndex(centerIndex + i);
            float setPosition = beforePosition + CELL_SIZE;

            cellRectTransform[index].localPosition = new Vector2(setPosition, 0);

            beforePosition = setPosition;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        onBeginDragPosition = Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
               _rectTransform,
               eventData.position,
               eventData.pressEventCamera,
               out onBeginDragPosition
           );

        onBeginStartScrollPosition = scrollPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out onDragPosition
            );

        // ドラッグ開始点と現在ドラッグ点との差分を取る
        Vector2 pointerDelta = onDragPosition - onBeginDragPosition;

        scrollPosition = (pointerDelta.x + onBeginStartScrollPosition) * SENSITIVITY;

        PositionUpdate();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    int GetLoopIndex(int index)
    {
        if (index < 0)
        {
            index = (CELL_COUNT - 1) + (index + 1) % CELL_COUNT;
        }
        else if (index > CELL_COUNT - 1)
        {
            index = index % CELL_COUNT;
        }
        return index;
    }
}
