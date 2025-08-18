import * as axiosConfig from '@/utils/axiosClient';
import ConfigsRequest from '@/models/ConfigsRequest';

const getCommentsByObjectIdAndType = async (objectId, objectType) => {
    let url = '';
    if (objectType === 'product') {
        url = `/Comment/${objectId}`;
    } else if (objectType === 'combo') {
        url = `/Comment/combo/${objectId}`;
    } else {
        throw new Error('Invalid objectType provided for fetching comments.');
    }
    return await axiosConfig.getFromApi(url);
};

const addComment = async (commentData) => {
    // commentData will now contain either MaSP or MaCombo
    return await axiosConfig.postToApi('/Comment', commentData, ConfigsRequest.takeAuth());
};

const updateComment = async (commentId, commentData) => {
    return await axiosConfig.putToApi(`/Comment/${commentId}`, commentData, ConfigsRequest.takeAuth());
};

const deleteComment = async (commentId) => {
    return await axiosConfig.deleteFromApi(`/Comment/${commentId}`, ConfigsRequest.takeAuth());
};

export const commentService = {
    getCommentsByObjectIdAndType,
    addComment,
    updateComment,
    deleteComment,
};