#nullable enable
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainDrive : MonoBehaviour
{
   [Header("Managers")] [SerializeField] SingletonSpawner singletonSpawnerPrefab = null!;

   [Header("Views")]
   [SerializeField] TitleView titleViewPrefab = null!;

public void Start()
   {
      Initialize();
   }

   void Initialize()
   {
      var singletonSpawner = Instantiate(singletonSpawnerPrefab);
      singletonSpawner.Initialize();

      var titleView = CreateTitleView();
      ViewManager.Instance.PushView(titleView);
   }

   #region Title View

   TitleView CreateTitleView()
   {
      var view = Instantiate(titleViewPrefab);
      view.Initialize();
      view.OnStartButtonPressed += HandleTitleViewStartButton;
      return view;
   }

   void HandleTitleViewStartButton()
   {
      ViewManager.Instance.ClearStack();
      SceneManager.LoadScene("GameScene");
   }
   
   #endregion
}
