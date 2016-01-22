using UnityEngine;
using UnityEngine.Purchasing;

public class StoreManager : MonoBehaviour, IStoreListener {

    public string productID = string.Empty;
    public string productID_iOS = string.Empty;
    public string productID_Android = string.Empty;

    private IStoreController _storeController = null;

    private IExtensionProvider _extensionProvider = null;

    public void Start () {
        this.Initialize();
    }

    /// 初期化処理をキック
    public void Initialize() {
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        // プロダクトを追加
        builder.AddProduct(
            // 各ストアのプロダクトIDを紐付けるためのエイリアス的なID
            this.productID,
            // ココでは仮に Consumable としているが、NonConsumable や Subscription も選択可
            ProductType.Consumable,
            // プロダクトID に対して、どのストアでのプロダクトなのかを紐付ける
            new IDs() {
                { productID_iOS, AppleAppStore.Name },
                { productID_Android, GooglePlay.Name },
                // プロダクトID が同一の場合には以下のような書き方も可能
                // { "tv.kidsstar.iap.HogeFugaProduct", AppleAppStore.Name, GooglePlay.Name }
            }
        );
        // 初期化
        // 第一引数は IStoreListener のインスタンスであれば良いので、勿論 this じゃなくても OK
        UnityPurchasing.Initialize(this, builder);
    }

    /// 初期化済かどうかをチェック
    public bool IsInitialized() {
        return null != this._storeController && null != this._extensionProvider;
    }

    /// 購入処理をキック
    public void Purchase() {
        if (!this.IsInitialized()) {
            Debug.LogError("初期化されてない");
            return;
        }
        // エイリアス的なプロダクトIDでプロダクト情報を特定
        Product product = this._storeController.products.WithID(this.productID);
        if (null == product || !product.availableToPurchase) {
            Debug.LogError("プロダクトが無効");
            return;
        }
        this._storeController.InitiatePurchase(product);
    }

    /// リストア処理をキック
    public void Restore() {
        if (!this.IsInitialized()) {
            Debug.LogError("初期化されてない");
            return;
        }
        // Android の場合、自動的にリストアが走るので、iOS に限定して処理をキックする
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
            // リストア処理は Extension に実装されている
            // iOS の場合は、IAppleExtension の RestoreTransactions メソッドに Action デリゲートを渡す
            this._extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions((result) => {

                /* ココでリストア成功時の処理を行う */

            });
        }
    }

    /// 初期化成功時に呼ばれる
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        this._storeController = controller;
        this._extensionProvider = extensions;
    }

    /// 初期化失敗時に呼ばれる
    public void OnInitializeFailed(InitializationFailureReason error) {
        Debug.LogError("初期化失敗");
    }

    /// 購入成功時に呼ばれる
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) {
        if (args.purchasedProduct.definition.id.ToLower() != this.productID) {
            Debug.LogError("ID が違うよ");
        }

        /* ココで購入成功時の処理を行う */

        return PurchaseProcessingResult.Complete;
    }

    /// 購入失敗時に呼ばれる
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
        Debug.LogError("購入失敗");
    }
}
