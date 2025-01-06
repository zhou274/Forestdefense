﻿using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;

// Placing the Purchaser class in the CompleteProject namespace allows it to interact with ScoreManager, 
// one of the existing Survival Shooter scripts.
	// Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
	public class Purchaser : MonoBehaviour, IStoreListener
	{
        private static IStoreController m_StoreController;          // The Unity Purchasing system.
		private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

		// Product identifiers for all products capable of being purchased: 
		// "convenience" general identifiers for use with Purchasing, and their store-specific identifier 
		// counterparts for use with and outside of Unity Purchasing. Define store-specific identifiers 
		// also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)

		// General product identifiers for the consumable, non-consumable, and subscription products.
		// Use these handles in the code to reference which product to purchase. Also use these values 
		// when defining the Product Identifiers on the store. Except, for illustration purposes, the 
		// kProductIDSubscription - it has custom Apple and Google identifiers. We declare their store-
		// specific mapping to Unity Purchasing's AddProduct, below.
		public static string kProductIDConsumable =    "coin_pack1";   
		public static string kProductIDNonConsumable = "nonconsumable";
		public static string kProductIDSubscription =  "subscription"; 

		public string item1 = "coin_pack1";
		public int reward1 = 5000;
    public float price1 = 2.99f;

    public string item2 = "coin_pack2";
		public int reward2 = 12000;
    public float price2 = 4.99f;

    public string item3 = "coin_pack3";
    public int reward3 = 30000;
    public float price3 = 9.99f;

    [HideInInspector] public string item4 = "coin_pack4";
    [HideInInspector] public int reward4 = 1200;
    [HideInInspector] public string item5 = "coin_pack5";
    [HideInInspector] public int reward5 = 2500;

    public string removeAds = "remove_ads";
    public float removeAdsPrice = 0.99f;


        public AudioClip soundPurchased;

		// Apple App Store-specific product identifier for the subscription product.
		private static string kProductNameAppleSubscription =  "com.unity3d.subscription.new";

		// Google Play Store-specific product identifier subscription product.
		private static string kProductNameGooglePlaySubscription =  "com.unity3d.subscription.original"; 

		void Start()
		{
			// If we haven't set up the Unity Purchasing reference
			if (m_StoreController == null)
			{
				// Begin to configure our connection to Purchasing
				InitializePurchasing();
			}
		}

		public void InitializePurchasing() 
		{
			// If we have already connected to Purchasing ...
			if (IsInitialized())
			{
				// ... we are done here.
				return;
			}

			// Create a builder, first passing in a suite of Unity provided stores.
			var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

			// Add a product to sell / restore by way of its identifier, associating the general identifier
			// with its store-specific identifiers.
			builder.AddProduct(kProductIDConsumable, ProductType.Consumable);


			builder.AddProduct(item1, ProductType.Consumable);
			builder.AddProduct(item2, ProductType.Consumable);
        builder.AddProduct(item3, ProductType.Consumable);
        builder.AddProduct(item4, ProductType.Consumable);
        builder.AddProduct(item5, ProductType.Consumable);

        // Continue adding the non-consumable product.
        builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable);

			builder.AddProduct(removeAds, ProductType.NonConsumable);

			// And finish adding the subscription product. Notice this uses store-specific IDs, illustrating
			// if the Product ID was configured differently between Apple and Google stores. Also note that
			// one uses the general kProductIDSubscription handle inside the game - the store-specific IDs 
			// must only be referenced here. 
			builder.AddProduct(kProductIDSubscription, ProductType.Subscription, new IDs(){
				{ kProductNameAppleSubscription, AppleAppStore.Name },
				{ kProductNameGooglePlaySubscription, GooglePlay.Name },
			});

			// Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
			// and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
			UnityPurchasing.Initialize(this, builder);
		}


		private bool IsInitialized()
		{
			// Only say we are initialized if both the Purchasing references are set.
			return m_StoreController != null && m_StoreExtensionProvider != null;
		}


		public void BuyItem1()
		{
			// Buy the consumable product using its general identifier. Expect a response either 
			// through ProcessPurchase or OnPurchaseFailed asynchronously.
			BuyProductID(item1);
		}

		public void BuyItem2()
		{
			// Buy the consumable product using its general identifier. Expect a response either 
			// through ProcessPurchase or OnPurchaseFailed asynchronously.
			BuyProductID(item2);
		}

    public void BuyItem3()
    {
        // Buy the consumable product using its general identifier. Expect a response either 
        // through ProcessPurchase or OnPurchaseFailed asynchronously.
        BuyProductID(item3);
    }

    public void BuyItem4()
    {
        // Buy the consumable product using its general identifier. Expect a response either 
        // through ProcessPurchase or OnPurchaseFailed asynchronously.
        BuyProductID(item4);
    }

    public void BuyItem5()
    {
        // Buy the consumable product using its general identifier. Expect a response either 
        // through ProcessPurchase or OnPurchaseFailed asynchronously.
        BuyProductID(item5);
    }

    public void BuyConsumable()
		{
			// Buy the consumable product using its general identifier. Expect a response either 
			// through ProcessPurchase or OnPurchaseFailed asynchronously.
			BuyProductID(kProductIDConsumable);
		}


		public void BuyNonConsumable()
		{
			// Buy the non-consumable product using its general identifier. Expect a response either 
			// through ProcessPurchase or OnPurchaseFailed asynchronously.
			BuyProductID(kProductIDNonConsumable);
		}

		public void BuyRemoveAds()
		{
			// Buy the non-consumable product using its general identifier. Expect a response either 
			// through ProcessPurchase or OnPurchaseFailed asynchronously.
			BuyProductID(removeAds);
		}


		public void BuySubscription()
		{
			// Buy the subscription product using its the general identifier. Expect a response either 
			// through ProcessPurchase or OnPurchaseFailed asynchronously.
			// Notice how we use the general product identifier in spite of this ID being mapped to
			// custom store-specific identifiers above.
			BuyProductID(kProductIDSubscription);
		}


		void BuyProductID(string productId)
		{
			// If Purchasing has been initialized ...
			if (IsInitialized())
			{
				// ... look up the Product reference with the general product identifier and the Purchasing 
				// system's products collection.
				Product product = m_StoreController.products.WithID(productId);

				// If the look up found a product for this device's store and that product is ready to be sold ... 
				if (product != null && product.availableToPurchase)
				{
					Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
					// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
					// asynchronously.
					m_StoreController.InitiatePurchase(product);
				}
				// Otherwise ...
				else
				{
					// ... report the product look-up failure situation  
					Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
				}
			}
			// Otherwise ...
			else
			{
				// ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
				// retrying initiailization.
				Debug.Log("BuyProductID FAIL. Not initialized.");
			}
		}


		// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
		// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
		public void RestorePurchases()
		{
			// If Purchasing has not yet been set up ...
			if (!IsInitialized())
			{
				// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
				Debug.Log("RestorePurchases FAIL. Not initialized.");
				return;
			}

			// If we are running on an Apple device ... 
			if (Application.platform == RuntimePlatform.IPhonePlayer || 
				Application.platform == RuntimePlatform.OSXPlayer)
			{
				// ... begin restoring purchases
				Debug.Log("RestorePurchases started ...");

				// Fetch the Apple store-specific subsystem.
				var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
				// Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
				// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
				apple.RestoreTransactions((result) => {
					// The first phase of restoration. If no more responses are received on ProcessPurchase then 
					// no purchases are available to be restored.
					Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
				});
			}
			// Otherwise ...
			else
			{
				// We are not running on an Apple device. No work is necessary to restore purchases.
				Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
			}
		}


		//  
		// --- IStoreListener
		//

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			// Purchasing has succeeded initializing. Collect our Purchasing references.
			Debug.Log("OnInitialized: PASS");

			// Overall Purchasing system, configured with products for this application.
			m_StoreController = controller;
			// Store specific subsystem, for accessing device-specific store features.
			m_StoreExtensionProvider = extensions;
		}


		public void OnInitializeFailed(InitializationFailureReason error)
		{
			// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
			Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
		}


		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) 
		{
			SoundManager.PlaySfx (soundPurchased);
			// A consumable product has been purchased by this user.
//			if (String.Equals (args.purchasedProduct.definition.id, kProductIDConsumable, StringComparison.Ordinal)) {
//				Debug.Log (string.Format ("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
//				// The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
////				ScoreManager.score += 100;
//				GlobalValue.SavedCoins += 100;
//				Debug.Log ("Add 100 stars");
//			}


//			else 
            if (String.Equals (args.purchasedProduct.definition.id, item1, StringComparison.Ordinal)) {
				Debug.Log (string.Format ("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
				// The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
				//				ScoreManager.score += 100;
				GlobalValue.SavedCoins += reward1;
				Debug.Log ("Purchase: " + reward1);
			}else if (String.Equals (args.purchasedProduct.definition.id, item2, StringComparison.Ordinal)) {
				Debug.Log (string.Format ("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
				// The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
				//				ScoreManager.score += 100;
				GlobalValue.SavedCoins += reward2;
				Debug.Log ("Purchase: " + reward2);
			}
        else if (String.Equals(args.purchasedProduct.definition.id, item3, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            // The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
            //				ScoreManager.score += 100;
            GlobalValue.SavedCoins += reward3;
            Debug.Log("Purchase: " + reward3);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, item4, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            // The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
            //				ScoreManager.score += 100;
            GlobalValue.SavedCoins += reward4;
            Debug.Log("Purchase: " + reward4);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, item5, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            // The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
            //				ScoreManager.score += 100;
            GlobalValue.SavedCoins += reward5;
            Debug.Log("Purchase: " + reward5);
        }
        // Or ... a non-consumable product has been purchased by this user.
        else if (String.Equals(args.purchasedProduct.definition.id, removeAds, StringComparison.Ordinal))
			{
				GlobalValue.RemoveAds = true;
				Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
				Debug.Log ("Ads removed");
			}


			else if (String.Equals(args.purchasedProduct.definition.id, kProductIDNonConsumable, StringComparison.Ordinal))
			{
				Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
				// TODO: The non-consumable item has been successfully purchased, grant this item to the player.
			}
			// Or ... a subscription product has been purchased by this user.
			else if (String.Equals(args.purchasedProduct.definition.id, kProductIDSubscription, StringComparison.Ordinal))
			{
				Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
				// TODO: The subscription item has been successfully purchased, grant this to the player.
			}
			// Or ... an unknown product has been purchased by this user. Fill in additional products here....
			else 
			{
				Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
			}

			// Return a flag indicating whether this product has completely been received, or if the application needs 
			// to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
			// saving purchased products to the cloud, and when that save is delayed. 
			return PurchaseProcessingResult.Complete;
		}


		public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
		{
			// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
			// this reason with the user to guide their troubleshooting actions.
			Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
		}
	}
#else
public class Purchaser : MonoBehaviour {
public void BuyItem1()
{
Debug.LogError ("You need to turn on IAP in Windown/Services tab to use this feature");
}

public void BuyItem2()
{
Debug.LogError ("You need to turn on IAP in Windown/Services tab to use this feature");
}

public void BuyItem3()
{
Debug.LogError ("You need to turn on IAP in Windown/Services tab to use this feature");
}

public void BuyRemoveAds()
{
Debug.LogError ("You need to turn on IAP in Windown/Services tab to use this feature");
}
} 
#endif