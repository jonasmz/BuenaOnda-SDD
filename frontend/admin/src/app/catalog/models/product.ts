export interface Product {
  id: string;
  name: string;
  description: string | null;
  imageUrl: string | null;
  hasImage: boolean;
  categoryId: string;
  price: number;
  isActive: boolean;
  isMarkedAvailable: boolean;
  isAvailable: boolean;
  isVisibleToPublic: boolean;
}

export interface UpdateProductInput {
  name: string;
  description: string | null;
  imageUrl: string | null;
  categoryId: string;
  price: number;
}

export interface CreateProductInput extends UpdateProductInput {
  isMarkedAvailable: boolean;
}
