// ProductDetail.jsx
import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import './ProductDetail.css';
import { getProductById } from '../services/api'; // Assuming you've created an API call
import './ProductCard.css';

function ProductDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchProduct = async () => {
      try {
        setLoading(true);
        const data = await getProductById(id);
        setProduct(data);
      } catch (err) {
        setError('Failed to load product details. Please try again later.');
      } finally {
        setLoading(false);
      }
    };
    fetchProduct();
  }, [id]);

  if (loading) return <div>Loading product...</div>;
  if (error) return <div>{error}</div>;

  return (
    <div className="product-detail-container">
      <button className="back-button" onClick={() => navigate('/')}>Back to Products</button>
      <div className="product-detail-card">
        <img src={product.imageUrl} alt={product.name} />
        <div className="product-info">
          <h2>{product.name}</h2>
          <p className="product-description">{product.description}</p>
          <div className="product-pricing">
            <span className="original-price">${product.price}</span>
            <span className="current-price">${product.salePrice}</span>
          </div>
          <div className={product.currentStock > 0 ? 'in-stock' : 'out-of-stock'}>
            {product.currentStock > 0 ? 'In Stock' : 'Out of Stock'}
          </div>
        </div>
      </div>
    </div>
  );
}

export default ProductDetail;
