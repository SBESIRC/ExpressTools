;;by 天华AI研究中心
;;天华效率工具平台-通用工具-辅助工具

;Multiple Scale:批量缩放：以各自的开始点（插入点等）为基准点，比例缩放多个实体
(defun c:THMSC(/ num fac i entname entlist bp)
    (princ "\n批量比例缩放\n")
    (setq oldcmd (getvar "cmdecho"))
    (setvar "cmdecho" 0)
    (setq sset (ssget))
    (setq num (sslength sset))
    (setq fac (getreal "\n输入比例系数:"))
    (setq i 0)
    (while (< i num)
        (progn
            (setq entname (ssname sset i))
            (setq entlist (entget entname))
            (setq bp (cdr (assoc 10 entlist)))
            (command "scale" entname "" bp fac)
            (setq i (1+ i))
        );progn
    );while
    (setvar "cmdecho" oldcmd)
);defun
