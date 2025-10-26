// ProductCard.jsx
import React from 'react';
import './ProductCard.css';

function ProductCard({ product }) {
  return (
    <div className="product-card">
      <div className="product-image">
        {product.isOnSale && <div className="sale-badge">Sale</div>}
        <img src={product.imageUrl} alt={product.name} />
      </div>
      <div className="product-info">
        <h3>{product.name}</h3>
        <div className="product-pricing">
          <span className="original-price">${product.price}</span>
          <span className="current-price">${product.salePrice}</span>
        </div>
        <div className={`product-stock ${product.currentStock > 0 ? 'in-stock' : 'out-of-stock'}`}>
          {product.currentStock > 0 ? 'In Stock' : 'Out of Stock'}
        </div>
      </div>
    </div>
  );
}

export default ProductCard;
