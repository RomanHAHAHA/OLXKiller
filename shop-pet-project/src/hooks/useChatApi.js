import { useState, useCallback } from 'react';
import { API_BASE_URL } from '../apiConfig';

export const useChatApi = () => {
  const [apiError, setApiError] = useState(null);
  const [isLoading, setIsLoading] = useState(false);

  const fetchChatHeader = useCallback(async (chatId) => {
    try {
      setIsLoading(true);
      const response = await fetch(`${API_BASE_URL}chats-api/api/chats/${chatId}/header`, {
        method: 'GET',
        credentials: 'include'
      });
      
      if (!response.ok) throw new Error('Failed to fetch chat header');
      
      const data = await response.json();
      return data.data;
    } catch (err) {
      setApiError(err.message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const fetchMessages = useCallback(async (chatId, cursor = null) => {
    try {
      setIsLoading(true);
      
      let url = `${API_BASE_URL}chats-api/api/chats/${chatId}/messages`;
      if (cursor) {
        url += `?cursor=${encodeURIComponent(cursor)}`;
      }
      console.log(url)
      const response = await fetch(url, {
        method: 'GET',
        credentials: 'include'
      });
      
      if (!response.ok) throw new Error('Failed to fetch messages');
      
      const data = await response.json();
      return {
        messages: data.cursorList.items,
        cursor: data.cursorList.cursor,
        hasMore: data.cursorList.hasMore
      };
    } catch (err) {
      setApiError(err.message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const sendMessage = useCallback(async (chatId, messageContent) => {
    try {
      setIsLoading(true);
      const encodedMessage = encodeURIComponent(messageContent);
      const response = await fetch(
        `${API_BASE_URL}chats-api/api/chats/${chatId}/messages?content=${encodedMessage}`, 
        {
          method: 'POST',
          credentials: 'include'
        }
      );

      if (!response.ok) {
        const data = await response.json();
        throw new Error(data.title || 'Failed to send message');
      }
    } catch (err) {
      setApiError(err.message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const markMessageAsRead = useCallback(async (messageId) => {
    try {
      setIsLoading(true);
      const response = await fetch(
        `${API_BASE_URL}chats-api/api/messages/${messageId}/read`, 
        {
          method: 'POST',
          credentials: 'include'
        }
      );

      if (!response.ok) throw new Error('Failed to mark message as read');
    } catch (err) {
      setApiError(err.message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  return {
    fetchChatHeader,
    fetchMessages,
    sendMessage,
    markMessageAsRead,
    apiError,
    isLoading
  };
};