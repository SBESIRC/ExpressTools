;;by �컪AI�о�����
;;�컪Ч�ʹ���ƽ̨-ͨ�ù���-���ֱ�������ع���

;Match Text Content :��������ˢ�����������ݸĳ�Դ��������
(defun c:THMTC (/ txt1 txt2 ent)
    (princ "\n��������ˢ\n") 
    (setq ent (car (entsel "\nѡ��Դ���֣�")))
    (setq txt1 (entget ent))
    (setq txt1 (cdr (assoc 1 txt1)))
    (while t
        (setq ent (car (entsel "\nѡ��Ŀ�����֣�")))
        (setq ent (entget ent))
        (setq typ (cdr (assoc 0 enx)))
        (if (= "TCH_TEXT" typ)
            (progn
                (prompt "\n�ݲ�֧���������ֶ�����ը��CAD���ֺ�ʹ�á�")
            )
            (progn
                (setq ent 
                    (subst (cons 1 txt1) (assoc 1 ent) ent)
                )
                (entmod ent)
            )
        )
    );while
);defun
