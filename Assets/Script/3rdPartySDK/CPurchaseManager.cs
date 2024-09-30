using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if PURCHASE_MODULE_ENABLE && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
using System.Threading.Tasks;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

#if UNITY_IOS || UNITY_ANDROID
using UnityEngine.Purchasing.Security;
#endif // #if UNITY_IOS || UNITY_ANDROID

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Purchasing;
#endif // #if UNITY_EDITOR

/** 결제 관리자 */
public class CPurchaseManager : SingletonMono<CPurchaseManager>, IStoreListener, IDetailedStoreListener
{
	/** 결제 콜백 */
	private enum EPurchaseCallback
	{
		NONE = -1,
		PURCHASE,
		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams
	{
		// public List<string> m_oProductIDList;
		public Dictionary<string, int> m_oProductIDDic;
		public System.Action<CPurchaseManager, bool> m_oInitCallback;
	}

	#region 변수
	private bool m_bIsPurchasing = false;
	private List<string> m_oPurchaseProductIDList = new List<string>();

	private IStoreController m_oStoreController = null;
	private IExtensionProvider m_oExtensionProvider = null;
	private Dictionary<EPurchaseCallback, System.Action<CPurchaseManager, string, bool, string>> m_oCallbackDict = new Dictionary<EPurchaseCallback, System.Action<CPurchaseManager, string, bool, string>>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public bool IsInit { get; private set; } = false;
	#endregion // 프로퍼티

	#region IStoreListener
	/** 초기화 되었을 경우 */
	public virtual void OnInitialized(IStoreController a_oStoreController, IExtensionProvider a_oExtensionProvider)
	{
		m_oStoreController = a_oStoreController;
		m_oExtensionProvider = a_oExtensionProvider;

#if UNITY_EDITOR && (DEBUG || DEVELOPMENT_BUILD)
		StandardPurchasingModule.Instance().useFakeStoreAlways = true;
		StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.Default;
#endif // #if UNITY_EDITOR && (DEBUG || DEVELOPMENT_BUILD)

		this.IsInit = true;
		this.Params.m_oInitCallback?.Invoke(this, this.IsInit);
	}

	/** 초기화에 실패했을 경우 */
	public virtual void OnInitializeFailed(InitializationFailureReason a_eReason)
	{
		this.OnInitializeFailed(a_eReason, string.Empty);
	}

	/** 초기화에 실패했을 경우 */
	public virtual void OnInitializeFailed(InitializationFailureReason a_eReason, string? a_oMsg)
	{
#if UNITY_EDITOR || (UNITY_IOS || UNITY_ANDROID)
		this.Params.m_oInitCallback?.Invoke(this, false);
#endif // #if UNITY_EDITOR || (UNITY_IOS || UNITY_ANDROID)
	}

	/** 결제를 진행 중 일 경우 */
	public virtual PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs a_oArgs)
	{
		string oID = a_oArgs.purchasedProduct.definition.id;

		try
		{
			// 결제 중 일 경우
			if (m_bIsPurchasing)
			{
				this.AddPurchaseProductID(oID);
			}

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
			int nIsVerifyReceipt = GlobalTable.GetData<int>("typeReceiptControl");

			// 영수증 검증 모드 일 경우
			if(nIsVerifyReceipt >= 1) {
				var oValidator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
				var oPurchaseReceipts = oValidator.Validate(a_oArgs.purchasedProduct.receipt);

				// 결제 영수증이 유효 할 경우
				if (oPurchaseReceipts.ExIsValid())
				{
					this.HandlePurchaseResult(oID, true, a_oReceipt: a_oArgs.purchasedProduct.receipt);
				}
				else
				{
					this.HandlePurchaseResult(oID, false, a_bIsComplete: true);
				}

				return (m_bIsPurchasing && oPurchaseReceipts.ExIsValid()) ?
					PurchaseProcessingResult.Pending : PurchaseProcessingResult.Complete;
			}

			this.HandlePurchaseResult(oID, true, a_oReceipt: a_oArgs.purchasedProduct.receipt);
			return m_bIsPurchasing ? PurchaseProcessingResult.Pending : PurchaseProcessingResult.Complete;
#else
			this.HandlePurchaseResult(oID, true, a_oReceipt: a_oArgs.purchasedProduct.receipt);
			return m_bIsPurchasing ? PurchaseProcessingResult.Pending : PurchaseProcessingResult.Complete;
#endif // #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		}
		catch (System.Exception oException)
		{
			this.HandlePurchaseResult(oID, false, a_bIsComplete: true);
		}

		return PurchaseProcessingResult.Complete;
	}

	/** 결제에 실패했을 경우 */
	public virtual void OnPurchaseFailed(Product a_oProduct, PurchaseFailureReason a_eReason)
	{
		this.OnPurchaseFailed(a_oProduct,
			new PurchaseFailureDescription(a_oProduct.definition.id, a_eReason, $"{a_eReason}"));
	}

	/** 결제에 실패했을 경우 */
	public virtual void OnPurchaseFailed(Product a_oProduct, PurchaseFailureDescription a_oDesc)
	{
		bool bIsPurchaseProduct = this.IsPurchaseNonConsumableProduct(a_oProduct) || 
			a_oDesc.reason == PurchaseFailureReason.DuplicateTransaction;

		this.ExLateCallFuncRealtime((a_oSender) =>
		{
			string oReceipt = a_oProduct.hasReceipt ? a_oProduct.receipt : string.Empty;
			this.HandlePurchaseResult(a_oProduct.definition.id, bIsPurchaseProduct, a_bIsComplete: !bIsPurchaseProduct, a_oReceipt: oReceipt);
		}, 0.15f);
	}
	#endregion // IStoreListener

	#region 함수
	/** 초기화 */
	public virtual async void Init(STParams a_stParams)
	{
		// 초기화 되었을 경우
		if (this.IsInit)
		{
			a_stParams.m_oInitCallback?.Invoke(this, this.IsInit);
		}
		else
		{
			this.Params = a_stParams;

#if DEBUG || DEVELOPMENT_BUILD
			string oEnvironmentName = "development";
#else
			string oEnvironmentName = "production";
#endif // #if DEBUG || DEVELOPMENT_BUILD

			var oOpts = new InitializationOptions();
			var oAsyncTask = UnityServices.InitializeAsync(oOpts.SetEnvironmentName(oEnvironmentName));

			await oAsyncTask;
			this.OnInit(oAsyncTask);
		}
	}

	/** 상품을 결제한다 */
	public void PurchaseProduct(string a_oID, System.Action<CPurchaseManager, string, bool, string> a_oCallback)
	{
		var oProduct = this.GetProduct(a_oID);
		bool bIsEnablePurchase = oProduct != null && oProduct.availableToPurchase;

		int nIsVerifyReceipt = GlobalTable.GetData<int>("typeReceiptControl");

		// 영수증 검증 모드 일 경우
		if (nIsVerifyReceipt >= 1)
		{
			bIsEnablePurchase = !m_bIsPurchasing && (oProduct != null && oProduct.availableToPurchase);
		}

		// 결제 가능 할 경우
		if (this.IsInit && bIsEnablePurchase)
		{
			m_bIsPurchasing = true;
			m_oCallbackDict.ExReplaceVal(EPurchaseCallback.PURCHASE, a_oCallback);

			// 결제 된 상품 일 경우
			if (m_oPurchaseProductIDList.Contains(a_oID) || this.IsPurchaseNonConsumableProduct(oProduct))
			{
				this.HandlePurchaseResult(a_oID, true);
			}
			else
			{
				m_oStoreController.InitiatePurchase(oProduct, "PurchaseMPurchaseProduct");
			}
		}
		else
		{
			a_oCallback?.Invoke(this, a_oID, false, string.Empty);
		}
	}

	/** 결제를 확정한다 */
	public void ConfirmPurchase(string a_oID, System.Action<CPurchaseManager, string, bool> a_oCallback)
	{
		var oProduct = this.GetProduct(a_oID);
		bool bIsEnableConfirm = m_bIsPurchasing && (oProduct != null && oProduct.availableToPurchase);

		m_oStoreController.ConfirmPendingPurchase(oProduct);

		this.ExLateCallFuncRealtime((a_oSender) =>
		{
			this.HandlePurchaseResult(a_oID, this.IsInit && bIsEnableConfirm, false, true);
			a_oCallback?.Invoke(this, a_oID, this.IsInit && bIsEnableConfirm);
		}, 0.15f);
	}

	/** 결제를 거부한다 */
	public void RejectPurchase(string a_oID, System.Action<CPurchaseManager, string, bool> a_oCallback)
	{
		var oProduct = this.GetProduct(a_oID);
		bool bIsEnableReject = m_bIsPurchasing && (oProduct != null && oProduct.availableToPurchase);

		this.ConfirmPurchase(a_oID, (a_oSender, a_oProductID, a_bIsSuccess) =>
		{
			a_oCallback?.Invoke(this, a_oID, this.IsInit && bIsEnableReject);
		});
	}

	/** 초기화 되었을 경우 */
	private void OnInit(Task a_oTask)
	{
		// 초기화에 실패했을 경우
		if (!a_oTask.ExIsCompleteSuccess())
		{
			return;
		}

		var oProductDefinitionList = new List<ProductDefinition>();

		// for (int i = 0; i < this.Params.m_oProductIDList.Count; ++i)
		foreach ( KeyValuePair<string, int> t in this.Params.m_oProductIDDic )
		{
			//oProductDefinitionList.ExAddVal(new ProductDefinition(this.Params.m_oProductIDList[i], ProductType.Consumable));
			oProductDefinitionList.ExAddVal(new ProductDefinition(t.Key, ProductType.Consumable));
		}

		var oModule = StandardPurchasingModule.Instance();
		UnityPurchasing.Initialize(this, ConfigurationBuilder.Instance(oModule).AddProducts(oProductDefinitionList));
	}

	/** 결제 결과를 처리한다 */
	private void HandlePurchaseResult(string a_oProductID,
		bool a_bIsSuccess, bool a_bIsInvoke = true, bool a_bIsComplete = false, string a_oReceipt = "")
	{
		// 완료 되었을 경우
		if (a_bIsComplete)
		{
			m_bIsPurchasing = false;
			this.RemovePurchaseProductID(a_oProductID);

			// 결제하면 보너스로 광고 제거 티켓을 준다
			if ( a_bIsSuccess )
            {
				uint key = GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey");
				int count = this.Params.m_oProductIDDic[a_oProductID];

				if ( count > 0 )
					StartCoroutine(GameManager.Singleton.AddItemCS(key, count));
			}
		}

		// 콜백 호출이 가능 할 경우
		if (a_bIsInvoke)
		{
			m_oCallbackDict.GetValueOrDefault(EPurchaseCallback.PURCHASE)?.Invoke(this, a_oProductID, a_bIsSuccess, a_oReceipt);
		}
	}

#if UNITY_EDITOR && UNITY_ANDROID
	/** 초기화 */
	[InitializeOnLoadMethod]
	public static void EditorInitialize()
	{
#if ANDROID_AMAZON_PLATFORM
		UnityPurchasingEditor.TargetAndroidStore(AppStore.AmazonAppStore);
#else
		UnityPurchasingEditor.TargetAndroidStore(AppStore.GooglePlay);
#endif // #if ANDROID_AMAZON_PLATFORM
	}
#endif // #if UNITY_EDITOR && UNITY_ANDROID
	#endregion // 함수

	#region 접근 함수
	/** 비소모 상품 결제 여부를 검사한다 */
	public bool IsPurchaseNonConsumableProduct(string a_oID)
	{
		return this.IsInit ? this.IsPurchaseNonConsumableProduct(this.GetProduct(a_oID)) : false;
	}

	/** 비소모 상품 결제 여부를 검사한다 */
	public bool IsPurchaseNonConsumableProduct(Product a_oProduct)
	{
		return this.IsInit && (a_oProduct.hasReceipt && a_oProduct.definition.type == ProductType.NonConsumable);
	}

	/** 상품을 반환한다 */
	public Product GetProduct(string a_oID)
	{
		return this.IsInit ? m_oStoreController.products.WithID(a_oID) : null;
	}

	/** 결제 상품 식별자를 추가한다 */
	private void AddPurchaseProductID(string a_oID)
	{
		m_oPurchaseProductIDList.ExAddVal(a_oID);
	}

	/** 결제 상품 식별자를 제거한다 */
	private void RemovePurchaseProductID(string a_oID)
	{
		m_oPurchaseProductIDList.ExRemoveVal(a_oID);
	}
	#endregion // 접근 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(Dictionary<string, int> a_oProductIDDic, System.Action<CPurchaseManager, bool> a_oInitCallback)
	{
		return new STParams()
		{
			m_oProductIDDic = a_oProductIDDic,
			m_oInitCallback = a_oInitCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}
#endif // #if PURCHASE_MODULE_ENABLE && (UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID)
