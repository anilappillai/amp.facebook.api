variable "aws_region" {
  description = "AWS region where resources are deployed"
  type        = string
  default     = "us-east-1"
}

variable "eks_cluster_name" {
  description = "Name of the EKS cluster"
  type        = string
}

variable "kubernetes_namespace" {
  description = "Kubernetes namespace where the app is deployed"
  type        = string
  default     = "amp-facebook-api"
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "common_tags" {
  description = "Tags applied to all AWS resources"
  type        = map(string)
  default = {
    Project     = "amp-facebook-api"
    ManagedBy   = "terraform"
  }
}
